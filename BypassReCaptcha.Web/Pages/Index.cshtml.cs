using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BypassReCaptcha.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace BypassReCaptcha.Web.Pages
{
    public class IndexModel : PageModel
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public bool BypassReCaptcha => Boolean.Parse(HttpContext.Session.GetString("BypassReCaptcha") ?? "false");
        public string ReCaptchaSiteKey { get; set; }
        private string ReCaptchaSecretKey { get; set; }

        [BindProperty]
        public ContactFormViewModel ContactFormViewModel { get; set; }

        public IndexModel(IConfiguration configuration)
        {
            ReCaptchaSiteKey = configuration.GetValue<string>("ReCaptcha.SiteKey");
            ReCaptchaSecretKey = configuration.GetValue<string>("ReCaptcha.SecretKey");
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || !await ValidateReCaptcha())
            {
                return Page();
            }

            return RedirectToPage("./ThankYou");
        }

        private async Task<bool> ValidateReCaptcha()
        {
            if (BypassReCaptcha)
            {
                return true;
            }

            if (Request.Form.TryGetValue("g-recaptcha-response", out StringValues reCaptchaResponse))
            {
                var formResult = new FormUrlEncodedContent(new Dictionary<string, string>(){
                    {"secret", ReCaptchaSecretKey},
                    {"response", reCaptchaResponse.First()},
                    {"remoteip", Request.HttpContext.Connection.RemoteIpAddress.ToString()}
                });
                var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", formResult);
                using var responseContentStream = await response.Content.ReadAsStreamAsync();
                var json = await JsonDocument.ParseAsync(responseContentStream);
                var success = json.RootElement.GetProperty("success").GetBoolean();
                if (!success)
                {
                    ModelState.AddModelError("InvalidReCaptcha", "Solve the captcha challenge");
                    return false;
                }

                return success;
            }
            else
            {
                ModelState.AddModelError("InvalidReCaptcha", "Solve the captcha challenge");
                return false;
            }
        }
    }
}
