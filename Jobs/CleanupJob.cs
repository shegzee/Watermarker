using HangfireWatermarker.Repositories;
using Microsoft.Extensions.Configuration;

namespace HangfireWatermarker.Jobs
{
    public class CleanupJob
    {
        private readonly IJobStatusRepository _jobStatusRepository;
        private readonly IConfiguration _configuration;

        public CleanupJob(IJobStatusRepository jobStatusRepository, IConfiguration configuration)
        {
            _jobStatusRepository = jobStatusRepository;
            _configuration = configuration;
        }

        public void CleanExpiredJobs()
        {
            DateTime expiryTime = DateTime.Now;
            var jobExpiryMinutes = _configuration.GetValue<int>("Settings:JobExpiryMinutes", 30);
            var outdatedJobItems = _jobStatusRepository.GetOutdatedJobItems(jobExpiryMinutes, expiryTime);

            var inputFilesDirectory = _configuration.GetValue<string>("FileDirectories:Input");
            var outputFilesDirectory = _configuration.GetValue<string>("FileDirectories:Output");

            foreach (var jobItem in outdatedJobItems)
            {
                var inputFile = Path.Combine(inputFilesDirectory, jobItem.InputFileName);
                var watermarkFile = Path.Combine(inputFilesDirectory, jobItem.WatermarkFileName);
                var outputFile = Path.Combine(outputFilesDirectory, jobItem.ResultFileName);
                File.Delete(inputFile);
                File.Delete(watermarkFile);
                File.Delete(outputFile);
            }
            _jobStatusRepository.DeleteOutdatedJobItems(jobExpiryMinutes, expiryTime);
        }
    }
}
