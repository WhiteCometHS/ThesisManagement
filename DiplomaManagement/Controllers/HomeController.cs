using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using DiplomaManagement.Models;
using DiplomaManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Localization;
using System.Diagnostics;

namespace DiplomaManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        private readonly InstituteRepository _instituteRepository;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, InstituteRepository instituteRepository)
        {
            _logger = logger;
            _context = context;
            _instituteRepository = instituteRepository;
        }

        public IActionResult SetLanguage(string culture, string returnUrl = "/")
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Index()
        {
            List<Institute> institutes = await _instituteRepository.GetAvailableInstitutes();

            ViewBag.Institutes = new SelectList(institutes, "Id", "Name");
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

        [HttpPost]
        public IActionResult RenderToastNotification([FromBody] ToastNotificationViewModel toastModel)
        {
            return PartialView("_ToastNotificationPartial", toastModel);
        }
    }
}
