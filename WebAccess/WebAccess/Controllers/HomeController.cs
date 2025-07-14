// WebAccess/Controllers/HomeController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevExpress.ExpressApp;
using ASPNetCore_WebAccess; // Kendi WebAccess kütüphanenizin namespace'i

// DevExpress ve Microsoft'un Controller sınıfları çakıştığı için tam yolu belirtiyoruz.
namespace WebAccess.Controllers
{
    // API endpoint'i olduğu için ControllerBase'den türetmek daha doğrudur.
    public class HomeController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(ILogger<HomeController> logger, IObjectSpaceFactory objectSpaceFactory, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _objectSpaceFactory = objectSpaceFactory;
            _serviceProvider = serviceProvider;
        }

        [Route("~/isbitek-handler")]
        [AllowAnonymous]
        [HttpGet, HttpPost]
        public Microsoft.AspNetCore.Mvc.ContentResult Index()
        {
            _logger.LogInformation("isbitek-handler endpoint'ine yeni bir istek geldi.");
            try
            {
                // ILogger<WebAccessRun> servisini IServiceProvider üzerinden alıyoruz.
                var webAccessRunLogger = _serviceProvider.GetRequiredService<ILogger<WebAccessRun>>();

                // WebAccessRun'ı başlatırken gereken servisleri ona veriyoruz.
                var webAccess_Run = new WebAccessRun(HttpContext, _objectSpaceFactory, webAccessRunLogger);

                // Oluşturulan yanıtı panele geri gönderiyoruz.
                string response = webAccess_Run.WebAccess.GetResponse();
                _logger.LogInformation("Panele gönderilen yanıt: {Response}", response);

                // ContentResult, ControllerBase'den gelir. Artık hata vermeyecektir.
                return Content(response, "text/plain; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "isbitek-handler işlenirken kritik bir hata oluştu.");
                return Content("", "text/plain; charset=utf-8");
            }
        }
    }
}