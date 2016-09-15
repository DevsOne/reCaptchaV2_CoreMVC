using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace reCaptchaV2_CoreMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AjaxRecaptcha()
        {
            return View();
        }

        public PartialViewResult AjaxRCPartial()
        {
            return PartialView();
        }

        public IActionResult TwoViews()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ValidateReCaptcha(string captcha)
        {
            ReCaptchaClass rc = new ReCaptchaClass();
            string EncodedResponse = Request.Form["g-Recaptcha-Response"];
            var response = await rc.Validate(EncodedResponse);
            bool IsCaptchaValid = response.ToLower() == "true" ? true : false;

            if (IsCaptchaValid)
            {
                var msg = "Thank You!";
                if (captcha == "rc1") { ViewBag.rc1 = msg; }
                else if (captcha == "rc2") { ViewBag.rc2 = msg; }
                else { ModelState.AddModelError("reCaptcha", msg); }

                if (IsAjaxRequest(Request)) { return PartialView("AjaxRCPArtial"); }
                else { return View("TwoViews"); }

            }
            else
            {
                var msg = "Please verify that you are a human!";
                if (captcha == "rc1") { ViewBag.rc1 = msg; }
                else if (captcha == "rc2") { ViewBag.rc2 = msg; }
                else { ModelState.AddModelError("reCaptcha", msg); }

                if (IsAjaxRequest(Request)) { return PartialView("AjaxRCPArtial"); }
                else { return View("TwoViews"); }

            }
        }
        public bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null) { throw new ArgumentNullException("request"); }
            if (request.Headers != null) { return request.Headers["X-Requested-With"] == "XMLHttpRequest"; }
            return false;
        }


    }


    public class ReCaptchaClass
    {
        private string m_Success;

        [JsonProperty("success")]
        public string Success
        {
            get { return m_Success; }
            set { m_Success = value; }
        }

        private List<string> m_ErrorCodes;

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes
        {
            get { return m_ErrorCodes; }
            set { m_ErrorCodes = value; }
        }

        public IConfiguration Configuration { get; set; }
        public ReCaptchaClass()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
        }


        public async Task<string> Validate(string EncodedResponse)
        {
            var client = new System.Net.Http.HttpClient();

            string PrivateKey = Configuration["AppSettings:RcPrivate"]; //You can put it in your web.config file.
            //string PrivateKey = "6LdfGyATAAAAAJ_abXDm3amRt1kqEeLXdhgydRYv";

            var GoogleReply = await client.GetStringAsync(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", PrivateKey, EncodedResponse));

            var captchaResponse = JsonConvert.DeserializeObject<ReCaptchaClass>(GoogleReply);

            return captchaResponse.Success;
        }
    }
}
