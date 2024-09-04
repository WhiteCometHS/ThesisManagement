using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using DiplomaManagement.Interfaces;
using DiplomaManagement.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace DiplomaManagement.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IThesisRepository _thesisRepository;
        private readonly IStringLocalizer<SharedResource> _htmlLocalizer;

        public EnrollmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService, IThesisRepository thesisRepository, IStringLocalizer<SharedResource> htmlLocalizer)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _thesisRepository = thesisRepository;
            _htmlLocalizer = htmlLocalizer;
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(int thesisId)
        {
            var user = await _userManager.GetUserAsync(User);

            var student = await _context.Students
            .FirstOrDefaultAsync(p => p.StudentUserId == user.Id);

            if (student != null) 
            {
                var createdEnrollment = await _context.Enrollments
                    .Where(e => e.ThesisId == thesisId && e.StudentId == student.Id)
                    .AnyAsync();

                if (createdEnrollment)
                {
                    _notificationService.AddNotification($"EnrollmentCreationError_{User.Identity.Name}", _htmlLocalizer["already-created-enrollment"]);
                }
                else
                {
                    Thesis? thesis = await _context.Theses.FirstOrDefaultAsync(t => t.Id == thesisId);
                    
                    if (thesis == null)
                    {
                        return NotFound();
                    }

                    thesis.Status = ThesisStatus.Requested;

                    Enrollment enrollment = new Enrollment
                    {
                        ThesisId = thesisId,
                        StudentId = student.Id
                    };

                    await _context.AddAsync(enrollment);
                    await _context.SaveChangesAsync();
                    _notificationService.AddNotification($"EnrollmentCreated_{User.Identity.Name}", _htmlLocalizer["enrollment-created-success"]);
                }
            }

            return RedirectToAction("AvailableTheses", "Thesis");
        }

        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> ActiveEnrollments()
        {
            var user = await _userManager.GetUserAsync(User);

            var enrollments = await _context.Enrollments
                .Include(e => e.Thesis)
                    .ThenInclude(t => t.Promoter)
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Where(e => e.Thesis.Promoter.PromoterUserId == user.Id)
                .ToListAsync();

            return View(enrollments);
        }

        [Authorize(Roles = "Promoter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEnrollment(int id)
        {
            Enrollment? enrollment = await _context.Enrollments
                .Include(e => e.Thesis)
                    .ThenInclude(t => t.Enrollments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment != null)
            {
                // enrollment.Thesis.Status = ThesisStatus.InProgress;
                // enrollment.Thesis.StudentId = enrollment.StudentId;

                List<Enrollment> thesisEnrollments = await _context.Enrollments
                    .Where(e => e.StudentId == enrollment.StudentId)
                    .ToListAsync();

                thesisEnrollments.AddRange(enrollment.Thesis.Enrollments.Where(e => e.StudentId != enrollment.StudentId).ToList());

                await _thesisRepository.assignThesisToStudent(enrollment.Thesis, enrollment.StudentId, thesisEnrollments);

                //_context.Enrollments.RemoveRange(enrollment.Thesis.Enrollments);

                //_context.Update(enrollment.Thesis);

                //await _context.SaveChangesAsync();

                _notificationService.AddNotification($"SuccessfullAssigned_{User.Identity.Name}", _htmlLocalizer["assign-selected-student-success"]);
            }

            return RedirectToAction("ActiveEnrollments");
        }
    }
}
