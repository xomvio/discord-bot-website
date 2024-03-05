using Bitguard.Models;
using Bitguard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Bitguard.DiscordRazor;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net;

namespace Bitguard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration config;       //config aq

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            config = configuration;
        }
        public IActionResult ChangeLanguage(string culture)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)));
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Premium")]
        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult Premium()
        {
            BotActionCounter.Updater(config);
            return View();
        }

        [Route("Privacy")]
        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Faq")]
        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult Faq()
        {
            return View();
        }


		[Route("Dashboard")]
		[Authorize(AuthenticationSchemes = "Discord")]
		public IActionResult Dashboard()
        {
            string x = User.Claims.First(c => c.Type == "token").Value;
			DashboardModel model = new DashboardModel(config,x, Request.Query["s"]);

			return View(model);
        }

        [Route("SaveChanges")]
        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult SaveChanges()
        {
            ServerConfig serverConfig = new ServerConfig()
            {
                serverId = Request.Query["s"].ToString(),
                ntfChannel = Request.Query["channelSelect"].ToString(),
                susCheck = Request.Query["suscheck"] == "on" ? "1" : "0",
                blockSpam = Request.Query["spamblock"] == "on" ? "1" : "0",
                badWords = Request.Query["badwords"].ToString(),
                badWordsWarn = Request.Query["badwordwarn"].ToString()
            };
            DbActions db = new DbActions(config);
            db.UpdateServerConfig(serverConfig);
            return Redirect(Request.Headers.Referer);
        }

        [Route("Logout")]
        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }

        [Route("Communication")]
        public IActionResult Communication()
        {
            IPAddress? aypi = Request.HttpContext.Connection.RemoteIpAddress;
            string mail = Request.Query["mail"].ToString();
            string msg = Request.Query["message"].ToString();
            DbActions db = new DbActions(config);
            db.AddFeedback(aypi, mail, msg);
            return NoContent(); //204
        }

        //[Route("Error")]
        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = "Discord")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [IgnoreAntiforgeryToken]
        public IActionResult Error()
        {
            if (Request.Query["status"].ToString() == "403")
                Response.Redirect("/Error?status=404");

            var handler = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ErrorViewModel error;
            try
            {
                if (handler != null)
                    error = new ErrorViewModel(config, handler);
                else
                    error = new ErrorViewModel(Request.Query["status"].ToString());
            }
            catch (Exception)
            {
                _logger.LogError("Error cannot handled:\n" + handler.Error);
                return LocalRedirect("/Home/Error?status=409");
            }

            return View(error);
            //return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
