using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace BypassReCaptcha.Web.Pages
{
    public class BypassReCaptchaModel : PageModel
    {
        private string serverBypassSecret;
        public bool BypassSucceeded { get; set; }

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
                ModelState.AddModelError("wrongsecret", "Wrong secret 👎");
            }
            else
            {
                HttpContext.Session.SetString("BypassReCaptcha", "true");
                BypassSucceeded = true;
            }

            return Page();
        }
    }
}
