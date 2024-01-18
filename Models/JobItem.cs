using System.ComponentModel.DataAnnotations;

namespace HangfireWatermarker.Models
{
    public class JobItem
    {
        //public int Id { get; set; }
        [Key]
        public Guid JobId { get; set; }
        public JobStatus Status { get; set; }
        public string InputFileName { get; set; }
        public string WatermarkFileName { get; set; }
        public string? ResultFileName { get; set; }
        public int Downloads { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public enum JobStatus
    {
        Enqueued,
        Processing,
        Completed,
        Failed
    }
}
