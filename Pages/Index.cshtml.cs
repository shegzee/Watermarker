using Hangfire;
using HangfireWatermarker.Jobs;
using HangfireWatermarker.Models;
using HangfireWatermarker.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;

namespace Watermarker.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IJobStatusRepository _jobStatusRepository;
        private readonly WatermarkJob _watermarkJob;

        [BindProperty]
        public IFormFile InputFile { get; set; }
        public IFormFile WatermarkFile { get; set; }

        public List<JobItem> JobStatusList { get; set; }

        public IndexModel(IJobStatusRepository jobStatusRepository, WatermarkJob watermarkJob, IConfiguration configuration)
        {
            _jobStatusRepository = jobStatusRepository;
            _watermarkJob = watermarkJob;
            _configuration = configuration;
            JobStatusList = new List<JobItem>();
        }

        public void OnGet()
        {
            JobStatusList = _jobStatusRepository.GetAllJobItems();
        }

        public IActionResult OnPost()
        {
            if (InputFile != null && WatermarkFile != null)
            {
                try
                {
                    // Security: Validate file type and size
                    if (!IsValidFileType(InputFile) || !IsValidFileSize(InputFile))
                    {
                        TempData["ErrorMessage"] = "Invalid file type or size.";
                        return RedirectToPage("/Index");
                    }

                    if (!IsValidWatermarkFileType(WatermarkFile) || !IsValidWatermarkFileSize(WatermarkFile))
                    {
                        TempData["ErrorMessage"] = "Invalid watermark file type or size.";
                        return RedirectToPage("/Index");
                    }

                    // Save the uploaded file to a temporary location
                    //var filePath = Path.GetTempFileName();
                    var inputFilesDirectory = _configuration.GetValue<string>("FileDirectories:Input");
                    if (!Directory.Exists(inputFilesDirectory))
                    {
                        Directory.CreateDirectory(inputFilesDirectory);
                    }

                    var inputFilePath = Path.Combine(inputFilesDirectory, UniqueFileName(InputFile.FileName));
                    using (var stream = new FileStream(inputFilePath, FileMode.Create))
                    {
                        InputFile.CopyTo(stream);
                        //WatermarkFile.CopyTo(stream);
                    }

                    // copy uploaded watermark image file (?)
                    //var watermarkFilePath = Path.GetTempFileName();
                    var watermarkFilePath = Path.Combine(inputFilesDirectory, UniqueFileName(WatermarkFile.FileName));
                    using (var stream = new FileStream(watermarkFilePath, FileMode.Create))
                    {
                        //InputFile.CopyTo(stream);
                        WatermarkFile.CopyTo(stream);
                    }

                    // Generate a unique job ID
                    var jobId = System.Guid.NewGuid();

                    // Security: Sanitize file names to prevent directory traversal attacks
                    var sanitizedFileName = Path.GetFileName(inputFilePath);
                    var sanitizedWatermarkFileName = Path.GetFileName(watermarkFilePath);

                    // Update job status to enqueued
                    _jobStatusRepository.AddJobItem(new JobItem
                    {
                        JobId = jobId,
                        InputFileName = sanitizedFileName,
                        WatermarkFileName = sanitizedWatermarkFileName,
                        Status = JobStatus.Enqueued,
                    });

                    // output file path
                    var outputFilesDirectory = _configuration.GetValue<string>("FileDirectories:Output");
                    if (!Directory.Exists(outputFilesDirectory))
                    {
                        Directory.CreateDirectory(outputFilesDirectory);
                    }

                    var outputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath) + "_watermarked";
                    var fileExtension = GetFileExtension(sanitizedFileName);
                    var timestamp = DateTime.Now.ToFileTime();
                    var outputFilePath = Path.Combine(outputFilesDirectory, $"{UniqueFileName(outputFileNameWithoutExtension + "." + fileExtension)}");

                    // Enqueue the watermark job
                    BackgroundJob.Enqueue(() => _watermarkJob.Process(jobId, fileExtension, inputFilePath, watermarkFilePath, outputFilePath));

                    // Redirect to Index page
                    return RedirectToPage("/Index");
                }
                catch (System.Exception ex)
                {
                    // Log the exception
                    // Handle other exceptions as needed
                    TempData["ErrorMessage"] = "An error occurred while processing the file.";
                    return RedirectToPage("/Index");
                }
            }

            TempData["ErrorMessage"] = "No file selected.";
            return RedirectToPage("/Index");
        }

        private bool IsValidFileType(IFormFile file)
        {
            var allowedFileTypes = new[] { "pdf", "jpeg", "jpg", "png", "mp4" };
            var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            return allowedFileTypes.Contains(fileExtension, System.StringComparer.OrdinalIgnoreCase);
        }

        private bool IsValidWatermarkFileType(IFormFile file)
        {
            var allowedFileTypes = new[] { "jpeg", "jpg", "png", "webp", "avif", "gif" };
            var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            return allowedFileTypes.Contains(fileExtension, System.StringComparer.OrdinalIgnoreCase);
        }

        private bool IsValidFileSize(IFormFile file)
        {
            var fileSizeLimit = 100 * 1024 * 1024; // 100 MB
            return file.Length <= fileSizeLimit;
        }

        private bool IsValidWatermarkFileSize(IFormFile file)
        {
            var fileSizeLimit = 5 * 1024 * 1024; // 5 MB
            return file.Length <= fileSizeLimit;
        }

        private string GetFileExtension(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName).TrimStart('.');
            return fileExtension.ToLower();
        }

        private string Truncate(string s, int maxLength)
        {
            return s != null && s.Length > maxLength ? s.Substring(0, maxLength) : s;
        }

        private string UniqueFileName(string fileName)
        {
            var maxLength = _configuration.GetValue<int>("Settings:FileNameMaxLength", 255);
            var timestamp = $"{DateTime.Now.ToFileTime()}";
            var extension = GetFileExtension(fileName);
            fileName = Path.GetFileNameWithoutExtension(fileName);
            fileName = Truncate(fileName, maxLength - timestamp.Length - extension.Length) + timestamp + "." + extension;
            return fileName;
        }
    }
}