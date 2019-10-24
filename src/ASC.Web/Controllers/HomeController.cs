using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ASC.Web.Models;
using ASC.Web.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using ASC.Utilities;

namespace ASC.Web.Controllers
{
    public class HomeController : AnonymousController
    {

        private readonly IOptions<ApplicationSettings> _settings;

        public HomeController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public IActionResult Index()
        {
            // Set session
            HttpContext.Session.SetSession("Test", _settings.Value);
            // Get session
            var setting = HttpContext.Session.GetSession<ApplicationSettings>("Test");
            // Usage of IOptions
            ViewData["Title"] = _settings?.Value.ApplicationTitle;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Error(string id)
        {
            // we handled only the 404 status code. We can handle any other status codes similarly.
            if (id == "404")
                return View("NotFound");

            if ((id == "401") && (User.Identity.IsAuthenticated))
                return RedirectToPage("/AccessDenied", new { area = "Identity" });
            else
                return RedirectToAction("Login", "Account");

            // return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

    }
}
