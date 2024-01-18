using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Watermarker.Pages
{
    public class ErrorModel : PageModel
    {
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            ErrorMessage = TempData["ErrorMessage"]?.ToString() ?? "An error occurred while processing your request.";
        }
    }
}