using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Identity;
using DiplomaManagement.Models;
using DiplomaManagement.Services;
using Microsoft.AspNetCore.Authorization;

namespace DiplomaManagement.Controllers
{
    public class ThesisController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;

        public ThesisController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _notificationService = notificationService;
        }

        // GET: Thesis
        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var promoter = await _context.Promoters
            .Include(p => p.Theses)
                .ThenInclude(t => t.PdfFiles)
            .Include(p => p.Theses)
                .ThenInclude(t => t.PresentationFile)
            .FirstOrDefaultAsync(p => p.PromoterUserId == user.Id);

            return View(promoter.Theses);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AvailableTheses()
        {
            var user = await _userManager.GetUserAsync(User);

            var theses = await _context.Theses
                .Include(t => t.Promoter)
                .ThenInclude(p => p.User)
                .Where(t => t.Promoter.User.InstituteId == user.InstituteId)
                .ToListAsync();

            return View(theses);
        }

        // GET: Thesis/Details/5
        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thesis = await _context.Theses
                .Include(t => t.PdfFiles)
                .Include(t => t.PresentationFile)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thesis == null)
            {
                return NotFound();
            }

            return View(thesis);
        }

        // GET: Thesis/Create
        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            var promoter = await _context.Promoters
            .Include(p => p.Theses)
            .FirstOrDefaultAsync(p => p.PromoterUserId == user.Id);

            if (promoter != null)
            {
                if (promoter.Theses != null && promoter.Theses.Count >= promoter.ThesisLimit)
                {
                    _notificationService.AddNotification($"MaxThesisLimit_{User.Identity.Name}", "You have reached the maximum number of theses.");
                    return RedirectToAction("Index");
                }
            } 
            else
            {
                return Forbid();
            }

            ViewData["PromoterId"] = promoter.Id;
            return View();
        }

        // POST: Thesis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromoterThesisViewModel vm)
        {
            var thesis = new Thesis
            {
                Title = vm.Title,
                Description = vm.Description,
                PromoterId = vm.PromoterId,
                Status = ThesisStatus.Available
            };

            await _context.AddAsync(thesis);

            if (vm.PdfFile != null)
            {
                var originalFileName = Path.GetFileNameWithoutExtension(vm.PdfFile.FileName);
                var fileExtension = Path.GetExtension(vm.PdfFile.FileName);
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"{originalFileName}_{timeStamp}{fileExtension}";
                var path = _configuration.GetSection("FileManagement:SystemFileUploads").Value;
                var filePath = Path.Combine(path, fileName);

                var stream = new FileStream(filePath, FileMode.Create);
                await vm.PdfFile.CopyToAsync(stream);

                var uploadedFile = new PdfFile
                {
                    Uploaded = DateTime.Now,
                    FileType = vm.PdfFile.ContentType,
                    FileName = fileName,
                    PdfType = PdfType.example,
                    FilePath = filePath,
                    Extension = fileExtension,
                    Thesis = thesis,
                };

                await _context.PdfFiles.AddAsync(uploadedFile);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Thesis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thesis = await _context.Theses
                .Include(p => p.PdfFiles)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thesis == null)
            {
                return NotFound();
            }

            var viewModel = new PromoterThesisViewModel
            {
                Id = id,
                Title = thesis.Title,
                Description = thesis.Description,
                SystemFiles = thesis.PdfFiles,
                PromoterId = thesis.PromoterId,
            };
            // ViewData["PresentationFileId"] = new SelectList(_context.PresentationFiles, "Id", "Id", thesis.PresentationFileId);
            return View(viewModel);
        }

        // POST: Thesis/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PromoterThesisViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var thesis = new Thesis
                    {
                        Id = (int)vm.Id,
                        Title = vm.Title,
                        Description = vm.Description,
                        PromoterId = vm.PromoterId,
                    };

                    _context.Update(thesis);

                    if (vm.PdfFile != null)
                    {
                        var originalFileName = Path.GetFileNameWithoutExtension(vm.PdfFile.FileName);
                        var fileExtension = Path.GetExtension(vm.PdfFile.FileName);
                        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        var fileName = $"{originalFileName}_{timeStamp}{fileExtension}";
                        var path = _configuration.GetSection("FileManagement:SystemFileUploads").Value;
                        var filePath = Path.Combine(path, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await vm.PdfFile.CopyToAsync(stream);

                            var uploadedFile = new PdfFile
                            {
                                Uploaded = DateTime.Now,
                                FileType = vm.PdfFile.ContentType,
                                FileName = fileName,
                                PdfType = PdfType.example,
                                FilePath = filePath,
                                Extension = fileExtension,
                                Thesis = thesis,
                            };

                            await _context.PdfFiles.AddAsync(uploadedFile);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThesisExists((int)vm.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Edit), new { id = (int)vm.Id });
            }
            return View(vm);
        }

        // GET: Thesis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thesis = await _context.Theses
                .Include(t => t.PdfFiles)
                .Include(t => t.PresentationFile)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (thesis == null)
            {
                return NotFound();
            }

            return View(thesis);
        }

        // POST: Thesis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thesis = await _context.Theses.FindAsync(id);
            if (thesis != null)
            {
                _context.Theses.Remove(thesis);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Files/Download/5
        public async Task<IActionResult> Download(int id)
        {
            PdfFile? file = await _context.PdfFiles.FirstOrDefaultAsync(m => m.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            var filePath = file.FilePath;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, file.FileType, file.FileName);
        }

        // POST: Thesis/DeleteExamplePdf/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExamplePdf(int id)
        {
            PdfFile? file = await _context.PdfFiles.FirstOrDefaultAsync(m => m.Id == id && m.PdfType == PdfType.example);

            if (file == null)
            {
                return NotFound();
            }

            var filePath =  file.FilePath;
            int thesisId = file.ThesisId;

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.PdfFiles.Remove(file);
            await _context.SaveChangesAsync();
            // TempData[$"DeleteExamplePdf_{User.Identity.Name}"] = "File has been successfully deleted.";
            _notificationService.AddNotification($"DeleteExamplePdf_{User.Identity.Name}", "File has been successfully deleted.");
            return RedirectToAction(nameof(Edit), new { id = thesisId });
        }

        private bool ThesisExists(int id)
        {
            return _context.Theses.Any(e => e.Id == id);
        }
    }
}
