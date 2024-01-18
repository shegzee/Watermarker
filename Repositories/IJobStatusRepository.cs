using HangfireWatermarker.Models;

namespace HangfireWatermarker.Repositories
{
    public interface IJobStatusRepository
    {
        JobItem GetJobItem(Guid jobId);
        void AddJobItem(JobItem jobItem);
        void UpdateJobItem(JobItem jobItem);
        List<JobItem> GetAllJobItems();
        void DeleteOutdatedJobItems(int jobExpiryMinutes, DateTime jobExpiryTime);
        public List<JobItem> GetOutdatedJobItems(int jobExpiryMinutes, DateTime jobExpiryTime);
    }
}
