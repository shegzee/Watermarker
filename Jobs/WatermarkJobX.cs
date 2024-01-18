using HangfireWatermarker.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Drawing.Imaging;
using HangfireWatermarker.Repositories;

namespace HangfireWatermarker.Jobs
{
    public class WatermarkJobX
    {
        /*private readonly IJobStatusRepository _jobStatusRepository;

        public WatermarkJobX(IJobStatusRepository jobStatusRepository)
        {
            _jobStatusRepository = jobStatusRepository;
        }

        public void Process(Guid jobId, string fileType, string filePath, string watermarkPath)
        {
            switch (fileType.ToLower())
            {
                case "pdf":
                    ProcessPdf(jobId, filePath, watermarkPath);
                    break;

                case "video":
                    ProcessVideo(jobId, filePath, watermarkPath);
                    break;

                case "image":
                    ProcessImage(jobId, filePath, watermarkPath);
                    break;

                default:
                    throw new ArgumentException("Unsupported file type");
            }
        }

        private void ProcessPdf(Guid jobId, string filePath, string watermarkPath)
        {
            // Add watermark to PDF
            string outputFilePath = Path.Combine(GetOutputDirectory(), $"{jobId}_output.pdf");

            using (var pdfReader = new PdfReader(filePath))
            using (var pdfWriter = new PdfWriter(outputFilePath))
            using (var pdf = new PdfDocument(pdfReader, pdfWriter))
            {
                var document = new Document(pdf);

                // Add watermark image to each page
                var image = new Image(iText.Kernel.Pdf.Canvas.PdfCanvasUtil
                                        .GetImageFromBytes(File.ReadAllBytes(watermarkPath), pdf));

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

            // Update job status to completed
            UpdateJobStatus(jobId, JobStatus.Completed, Path.GetFileName(outputFilePath));
        }

        private void ProcessVideo(Guid jobId, string filePath, string watermarkPath)
        {
            // Use FFmpeg to add watermark to video
            string outputFilePath = Path.Combine(GetOutputDirectory(), $"{jobId}_output.mp4");

            // Example FFmpeg command: ffmpeg -i input.mp4 -i watermark.png -filter_complex "overlay=10:10" output.mp4
            var ffmpegCommand = $"ffmpeg -i {filePath} -i {watermarkPath} -filter_complex overlay=10:10 {outputFilePath}";

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

            // Update job status to completed
            UpdateJobStatus(jobId, JobStatus.Completed, Path.GetFileName(outputFilePath));
        }

        private void ProcessImage(Guid jobId, string filePath, string watermarkPath)
        {
            // Use System.Drawing for image processing
            string outputFilePath = Path.Combine(GetOutputDirectory(), $"{jobId}_output.jpg");

            using (var originalImage = System.Drawing.Image.FromFile(filePath))
            using (var watermarkImage = System.Drawing.Image.FromFile(watermarkPath))
            using (var graphics = Graphics.FromImage(originalImage))
            using (var watermarkGraphics = Graphics.FromImage(originalImage))
            {
                // Scale the watermark to a little smaller than the image size
                float scale = 0.8f;
                int width = (int)(originalImage.Width * scale);
                int height = (int)(originalImage.Height * scale);
                var scaledWatermark = new Bitmap(watermarkImage, new Size(width, height));

                // Add the scaled watermark to the original image
                graphics.DrawImage(scaledWatermark, new Point(originalImage.Width / 2 - width / 2, originalImage.Height / 2 - height / 2));
                // Save the processed image
                originalImage.Save(outputFilePath, ImageFormat.Jpeg);
            }


            // Update job status to completed
            UpdateJobStatus(jobId, JobStatus.Completed, Path.GetFileName(outputFilePath));
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

        private void UpdateJobStatus(Guid jobId, JobStatus status, string resultFileName)
        {
            // Update the job status in the database or another storage
            // You may use _jobStatusRepository.UpdateJobStatus(jobId, status, resultFileName);
            // Ensure to handle concurrency issues if multiple jobs might update the status simultaneously.
            JobItem jobItem = _jobStatusRepository.GetJobItem(jobId);
            if (jobItem == null)
            {
                _jobStatusRepository.AddJobItem(jobItem = new JobItem { JobId = jobId, Status = status, ResultFileName = resultFileName });
            }
            else
            {
                jobItem.Status = status;
                jobItem.ResultFileName = resultFileName;
                _jobStatusRepository.UpdateJobItem(jobItem);
            }
        }*/
    }

}
