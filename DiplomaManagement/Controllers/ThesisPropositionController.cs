using DiplomaManagement.Data;
using DiplomaManagement.Interfaces;
using DiplomaManagement.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
namespace DiplomaManagement.Controllers
{
    public class ThesisPropositionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResource> _htmlLocalizer;

        public ThesisPropositionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService, IStringLocalizer<SharedResource> htmlLocalizer)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _htmlLocalizer = htmlLocalizer;
        }

        // GET: ThesisProposition/Create
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            var student = await _context.Students
                .FirstOrDefaultAsync(p => p.StudentUserId == user.Id);

            if (student == null)
            {
                return Forbid();
            }

            ViewData["StudentId"] = student.Id;
            return View();
        }
    }
}