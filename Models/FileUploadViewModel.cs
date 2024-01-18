using System.ComponentModel.DataAnnotations;

namespace HangfireWatermarker.Models
{
    public class FileUploadViewModel
    {
        [Required]
        public IFormFile File { get; set; }

        // Add other properties as needed
    }

}
