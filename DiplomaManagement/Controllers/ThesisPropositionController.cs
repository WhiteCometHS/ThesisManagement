using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using DiplomaManagement.Interfaces;
using DiplomaManagement.Models;
using DiplomaManagement.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
namespace DiplomaManagement.Controllers
{
    public class ThesisPropositionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResource> _htmlLocalizer;

        public ThesisPropositionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, IConfiguration configuration, INotificationService notificationService, IStringLocalizer<SharedResource> htmlLocalizer)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _configuration = configuration;
            _notificationService = notificationService;
            _htmlLocalizer = htmlLocalizer;
        }

        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> Index()
        {
            var userClaims = User.Claims;
            var promoterIdClaim = userClaims.FirstOrDefault(c => c.Type == "PromoterId");

            if (promoterIdClaim == null)
            {
                return NotFound("User not found.");
            }
            int promoterId = int.Parse(promoterIdClaim.Value);

            List<ThesisProposition> propositions = await _context.ThesisPropositions
                .Where(p => p.PromoterId == promoterId)
                .ToListAsync();

            if(propositions != null) 
            {
                return View(propositions);
            }
            else 
                return NotFound();
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

            List<Promoter> directorUsers = _context.Promoters
                .Include(d => d.User!)
                    .ThenInclude(u => u.Institute)
                .Where(d => d.User!.Institute!.Id == user!.InstituteId)
                .ToList();


            List<SelectListItem> directorsSelectList = directorUsers.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.User.FirstName} {u.User.LastName}"
            }).ToList();

            ViewBag.Promoters = new SelectList(directorsSelectList, "Value", "Text");
            ViewBag.StudentId = student.Id;
            return View();
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThesisPropositionViewModel vm)
        {
            var thesisProposition = new ThesisProposition
            {
                Title = vm.Title,
                Description = vm.Description,
                StudentId = vm.StudentId,
                PromoterId = vm.PromoterId,
            };

            await _context.AddAsync(thesisProposition);

            if (vm.PdfFiles.Count() > 0)
            {
                foreach (IFormFile file in vm.PdfFiles)
                {
                    var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var fileExtension = Path.GetExtension(file.FileName);
                    var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var fileName = $"{originalFileName}_{timeStamp}{fileExtension}";
                    var path = Path.Combine(_env.WebRootPath, _configuration.GetSection("FileManagement:SystemFileUploads").Value ?? "");
                    var filePath = Path.Combine(path, fileName);

                    var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var uploadedFile = new PdfFile
                    {
                        Uploaded = DateTime.Now,
                        FileType = file.ContentType,
                        FileName = fileName,
                        PdfType = PdfType.example,
                        FilePath = filePath,
                        Extension = fileExtension,
                        ThesisProposition = thesisProposition,
                    };

                    await _context.PdfFiles.AddAsync(uploadedFile);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("AvailableTheses", "Thesis");
        }
    }
}