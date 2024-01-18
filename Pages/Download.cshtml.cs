using HangfireWatermarker.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Watermarker.Pages
{
    public class DownloadModel : PageModel
    {
        public string DownloadLink { get; set; }
        public string InputFileName { get; set; }
        public string ResultFileName { get; set; }

        private readonly IConfiguration _configuration;
        private readonly IJobStatusRepository _jobStatusRepository;

        public DownloadModel(IConfiguration configuration, IJobStatusRepository jobStatusRepository)
        {
            _configuration = configuration;
            _jobStatusRepository = jobStatusRepository;
        }

        public void OnGet(string jobId)
        {
            var outputFilesDirectory = _configuration.GetValue<string>("FileDirectories:Output");
            // Generate the download link based on the job ID
            // Example: DownloadLink = $"/api/files/download?jobId={jobId}";
            Guid jobIdGuid = Guid.Parse(jobId);
            var jobItem = _jobStatusRepository.GetJobItem(jobIdGuid);

            InputFileName = jobItem.InputFileName;
            ResultFileName = jobItem.ResultFileName;
            
            var outputFilePath = Path.Combine(outputFilesDirectory, jobItem.ResultFileName);
            // Set the download link to a placeholder for demonstration purposes
            // DownloadLink = "https://example.com/download/" + jobId;
            DownloadLink = $"/api/files/download/{jobId}";
            //DownloadLink = outputFilePath;
        }
    }
}