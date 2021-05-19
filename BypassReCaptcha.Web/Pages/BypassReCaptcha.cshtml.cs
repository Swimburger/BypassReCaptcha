using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace BypassReCaptcha.Web.Pages
{
    public class BypassReCaptchaModel : PageModel
    {
        private string serverBypassSecret;

        [BindProperty]
        public string BypassSecret { get; set; }

        public BypassReCaptchaModel(IConfiguration configuration)
        {
            serverBypassSecret = configuration.GetValue<string>("BypassSecret");
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if(BypassSecret != serverBypassSecret)
            {
                return Content("Wrong secret 👎");
            }
            else
            {
                HttpContext.Session.SetString("BypassReCaptcha", "true");
                return Content("Success! 👍");
            }
        }
    }
}
