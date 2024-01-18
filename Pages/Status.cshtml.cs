using HangfireWatermarker.Models;
using HangfireWatermarker.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Watermarker.Pages
{
    public class StatusModel : PageModel
    {
        private readonly IJobStatusRepository _jobStatusRepository;

        public StatusModel(IJobStatusRepository jobStatusRepository)
        {
            _jobStatusRepository = jobStatusRepository;
        }

        public string JobId { get; set; }
        public string Status { get; set; }
        public int ProgressPercentage { get; set; }

        public async Task OnGetAsync(string jobId)
        {
            var jobItem = _jobStatusRepository.GetJobItem(System.Guid.Parse(jobId));

            if (jobItem != null)
            {
                JobId = jobItem.JobId.ToString();
                Status = jobItem.Status.ToString();

                // Additional logic to calculate progress percentage based on your application requirements
                // For example, you might retrieve progress information from a storage system
                ProgressPercentage = CalculateProgressPercentage(jobItem);
            }
            else
            {
                // Job not found
                // Handle accordingly, e.g., redirect to an error page
                Response.Redirect("/Error");
            }
        }

        private int CalculateProgressPercentage(JobItem jobItem)
        {
            // Example: Calculate progress based on the number of completed steps out of total steps
            int totalSteps = Enum.GetValues(typeof(JobStatus)).Length; // Adjust based on your application's processing steps
            int completedSteps = (int)jobItem.Status; // Get this information from your processing logic
            return (int)(((float)completedSteps / totalSteps) * 100);
        }
    }
}