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

        public IActionResult Dashboard()
        {
            return View();
        }

    }
}
