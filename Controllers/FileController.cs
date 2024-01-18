using Hangfire;
using HangfireWatermarker.Jobs;
using HangfireWatermarker.Models;
using HangfireWatermarker.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HangfireWatermarker.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IJobStatusRepository _jobStatusRepository;
        private readonly WatermarkJob _watermarkJob;

        public FileController(IJobStatusRepository jobStatusRepository, WatermarkJob watermarkJob, IConfiguration configuration)
        {
            _jobStatusRepository = jobStatusRepository;
            _watermarkJob = watermarkJob;
            _configuration = configuration;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Security: Validate file type and size
                    if (!IsValidFileType(model.File) || !IsValidFileSize(model.File))
                    {
                        return BadRequest("Invalid file type or size.");
                    }

                    // Save the uploaded file to a temporary location
                    var filePath = Path.GetTempFileName();
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(stream);
                    }

                    // Generate a unique job ID
                    var jobId = Guid.NewGuid();

                    // Security: Sanitize file names to prevent directory traversal attacks
                    var sanitizedFileName = Path.GetFileName(model.File.FileName);

                    // Update job status to enqueued
                    _jobStatusRepository.AddJobItem(new JobItem
                    {
                        JobId = jobId,
                        Status = JobStatus.Enqueued
                    });

                    // output file path
                    var outputFilesDirectory = _configuration.GetValue<string>("FileDirectories:Output");
                    if (!Directory.Exists(outputFilesDirectory))
                    {
                        Directory.CreateDirectory(outputFilesDirectory);
                    }

                    var outputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(sanitizedFileName);
                    var fileExtension = GetFileType(sanitizedFileName);
                    var outputFilePath = Path.Combine(outputFilesDirectory, $"{outputFileNameWithoutExtension}_watermarked.{fileExtension}");

                    // Enqueue the watermark job
                    BackgroundJob.Enqueue(() => _watermarkJob.Process(jobId, GetFileType(sanitizedFileName), filePath, GetWatermarkPath(), outputFilePath));

                    // Return the job ID to the user
                    return Ok(new { JobId = jobId });
                }
                catch (Exception ex)
                {
                    // Log the exception
                    // Handle other exceptions as needed
                    return StatusCode(500, "Internal server error");
                }
            }

            // Model validation failed
            return BadRequest(ModelState);
        }

        [HttpGet("download/{jobId}")]
        public async Task<IActionResult> DownloadFile(string jobId)
        {
            var outputFilesDirectory = _configuration.GetValue<string>("FileDirectories:Output");
            // Generate the download link based on the job ID
            // Example: DownloadLink = $"/api/files/download?jobId={jobId}";
            Guid jobIdGuid = Guid.Parse(jobId);
            var jobItem = _jobStatusRepository.GetJobItem(jobIdGuid);
            var fileName = jobItem.ResultFileName;
            var outputFilePath = Path.Combine(outputFilesDirectory, fileName);

            // increment download count
            jobItem.Downloads += 1;
            _jobStatusRepository.UpdateJobItem(jobItem);

            byte[] fileBytes = GetFile(outputFilePath);
            
            return File(fileBytes, GetContentType(fileName), fileName);
        }

        string GetContentType(string filePath)
        {
            var fileExtension = GetFileType(filePath);
            if (fileExtension == null)
            {
                return "application/text";
            }
            var imageFileTypes = new[] { "jpeg", "jpg", "png" };
            var videoFileTypes = new[] { "mp4" };
            var pdfFileTypes = new[] { "pdf" };
            if (imageFileTypes.Contains(fileExtension)) return "image/"+fileExtension;
            if (videoFileTypes.Contains(fileExtension)) return "video/"+fileExtension;
            if (pdfFileTypes.Contains(fileExtension)) return "application/pdf";
            return "";
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

        // Security: Validate file type
        private bool IsValidFileType(IFormFile file)
        {
            var allowedFileTypes = new[] { "pdf", "jpeg", "jpg", "png", "mp4" }; // Add more allowed types as needed
            var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');

            return allowedFileTypes.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        // Security: Validate file size
        private bool IsValidFileSize(IFormFile file)
        {
            // Set a reasonable file size limit (e.g., 10MB)
            var fileSizeLimit = 10 * 1024 * 1024; // 10 MB
            return file.Length <= fileSizeLimit;
        }

        // Security: Get the file type based on extension
        private string GetFileType(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName).TrimStart('.');
            return fileExtension.ToLower();
        }

        // Security: Get the watermark path (update as needed)
        private string GetWatermarkPath()
        {
            // Example: return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "watermark.png");
            return "path/to/your/watermark.png";
        }
    }
}
