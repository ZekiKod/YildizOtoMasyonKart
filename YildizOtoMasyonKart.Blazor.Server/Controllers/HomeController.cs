using ASPNetCore_WebAccess;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebAccess.Models;

namespace WebAccess.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public string Index()
        {
            WebAccessRun WebAccess_Run = new WebAccessRun(HttpContext);
            Console.WriteLine(WebAccess_Run.WebAccess.GetResponse());
            return WebAccess_Run.WebAccess.GetResponse();
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
    }
}