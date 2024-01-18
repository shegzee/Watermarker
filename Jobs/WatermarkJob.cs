using FFMediaToolkit.Decoding;
using HangfireWatermarker.Models;
//using static System.Net.Mime.MediaTypeNames;
//using System.Drawing;
//using System.Reflection.Metadata;
//using System.Reflection.PortableExecutable;
//using System.Drawing.Imaging;
//using iTextSharp.text.pdf;
using HangfireWatermarker.Repositories;
using ImageMagick;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace HangfireWatermarker.Jobs
{
    public class WatermarkJob
    {
        private readonly IJobStatusRepository _jobStatusRepository;

        public WatermarkJob(IJobStatusRepository jobStatusRepository)
        {
            _jobStatusRepository = jobStatusRepository;
        }

        public void Process(Guid jobId, string fileExtension, string filePath, string watermarkPath, string outputPath)
        {
            try
            {
                var jobItem = _jobStatusRepository.GetJobItem(jobId);
                jobItem.Status = JobStatus.Processing;
                // Update job status to processing
                _jobStatusRepository.UpdateJobItem(jobItem);
                var originalFileName = jobItem.InputFileName;
                // Set the output file path
                //var outputPath = GetOutputFilePath(filePath, originalFileName, fileExtension);
                var fileType = GetFileType(fileExtension);

                // Apply watermark based on file type
                if (fileType.Equals("other", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FileLoadException("Unsupported File Type");
                }

                CreateOutputFile(outputPath);

                if (fileType.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyWatermarkToPdf(filePath, outputPath, watermarkPath);
                }
                else if (fileType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyWatermarkToImage(filePath, outputPath, watermarkPath);
                }
                else if (fileType.StartsWith("video", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyWatermarkToVideo(filePath, outputPath, watermarkPath);
                }
                

                jobItem = _jobStatusRepository.GetJobItem(jobId);
                jobItem.ResultFileName = Path.GetFileName(outputPath);
                jobItem.Status = JobStatus.Completed;

                // Update job status to completed
                _jobStatusRepository.UpdateJobItem(jobItem);
            }
            catch (Exception ex)
            {
                var jobItem = _jobStatusRepository.GetJobItem(jobId);
                // Log the exception
                // Handle other exceptions as needed

                // Update job status to failed
                jobItem.Status = JobStatus.Failed;
                _jobStatusRepository.UpdateJobItem(jobItem);

                // let Hangfire know it failed
                throw new InvalidOperationException("Job Failed/n" + ex.Message);
            }
        }

        private string GetFileType(string fileExtension)
        {
            var imageFileTypes = new[] { "jpeg", "jpg", "png", "gif", "avif", "webp" };
            var videoFileTypes = new[] { "mp4" };
            var pdfFileTypes = new[] { "pdf" };
            if (imageFileTypes.Contains(fileExtension)) return "image";
            if (videoFileTypes.Contains(fileExtension)) return "video";
            if (pdfFileTypes.Contains(fileExtension)) return "pdf";
            return "other";
        }

        private void CreateOutputFile(string filePath)
        {
            using (File.Create(filePath)) { }
        }

        private string GetOutputFilePath(string originalFilePath, string originalFileName, string fileType)
        {
            // Construct the output file path based on the original file path and type
            //var directory = Path.GetDirectoryName(originalFilePath);
            var directory = Path.GetDirectoryName(originalFilePath);
            directory = Path.Combine(directory, "output");
            //var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilePath);
            var fileNameWithoutExtension = originalFileName;
            var outputFileName = $"{fileNameWithoutExtension}_watermarked.{fileType}";
            return Path.Combine(directory, outputFileName);
        }

        private void ApplyWatermarkToPdf(string inputPath, string outputPath, string watermarkPath)
        {
            // Add watermark to PDF
            //string outputFilePath = Path.Combine(GetOutputDirectory(), $"{jobId}_output.pdf");

            using (var pdfReader = new PdfReader(inputPath))
            using (var pdfWriter = new PdfWriter(outputPath))
            using (var pdf = new PdfDocument(pdfReader, pdfWriter))
            {
                Document document = new Document(pdf);

                // fetch watermark
                ImageData imageData = ImageDataFactory.Create(watermarkPath);
                Image image = new Image(imageData);
                // Add watermark image to each page
                for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
                {
                    var pageSize = pdf.GetPage(i).GetPageSize();
                    image.SetFixedPosition(i, pageSize.GetWidth() / 2, pageSize.GetHeight() / 2, 100);

                    // Scale the watermark to a little smaller than the page size
                    float scale = 0.8f;
                    image.ScaleToFit(pageSize.GetWidth() * scale, pageSize.GetHeight() * scale);

                    document.Add(image);
                }
            }
        }

        private void ApplyWatermarkToImage(string inputPath, string outputPath, string watermarkPath)
        {
            using (var image = new MagickImage(inputPath))
            {
                using (var watermark = new MagickImage(watermarkPath))
                {
                    // Resize watermark to fit image
                    watermark.Resize(image.Width / 3, image.Height / 3);

                    // Position the watermark (adjust as needed)
                    var posX = (image.Width - watermark.Width) / 2;
                    var posY = (image.Height - watermark.Height) / 2;

                    image.Composite(watermark, posX, posY, CompositeOperator.Over);

                    // Save the watermarked image
                    image.Write(outputPath);
                }
            }
        }

        private void ApplyWatermarkToVideo(string inputPath, string outputPath, string watermarkPath)
        {
            // Use FFmpeg to add watermark to video
            // string outputFilePath = Path.Combine(GetOutputDirectory(), $"{jobId}_output.mp4");

            // Example FFmpeg command: ffmpeg -i input.mp4 -i watermark.png -filter_complex "overlay=10:10" output.mp4
            var ffmpegCommand = $"ffmpeg -i {inputPath} -i {watermarkPath} -filter_complex overlay=10:10 {outputPath}";

            // Run FFmpeg command
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };

            process.Start();

            using (var streamWriter = process.StandardInput)
            {
                if (streamWriter.BaseStream.CanWrite)
                {
                    streamWriter.WriteLine(ffmpegCommand);
                }
            }

            process.WaitForExit();

        }

        private string GetOutputDirectory()
        {
            string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "OutputFiles");

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            return outputDirectory;
        }
    }

}
