using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Identity;
using DiplomaManagement.Models;

using Microsoft.AspNetCore.Authorization;
using DiplomaManagement.Interfaces;
using DiplomaManagement.Services;
using DiplomaManagement.Repositories;

namespace DiplomaManagement.Controllers
{
    public class ThesisController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IThesisRepository _thesisRepository;

        public ThesisController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, IConfiguration configuration, INotificationService notificationService, IThesisRepository thesisRepository)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _configuration = configuration;
            _notificationService = notificationService;
            _thesisRepository = thesisRepository;
        }

        // GET: Thesis
        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            Promoter? promoter = await _context.Promoters
                .Include(p => p.Theses!)
                    .ThenInclude(t => t.PdfFiles)
                .Include(p => p.Theses!)
                    .ThenInclude(t => t.PresentationFile)
                .FirstOrDefaultAsync(p => p.PromoterUserId == user.Id);

            if(promoter != null) 
            {
                if (promoter.Theses == null)
                    return View(new List<Thesis>());
                else
                    return View(promoter.Theses);
            }
            else 
                return NotFound();
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AvailableTheses()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            Student? student = await _context.Students
                .Include(s => s.Thesis)
                .FirstOrDefaultAsync(p => p.StudentUserId == user.Id);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            List<Thesis> theses = await _context.Theses
                .Include(t => t.Promoter!)
                    .ThenInclude(p => p.User)
                .Include(t => t.Enrollments)   
                .Where(t => t.Promoter!.User!.InstituteId == user.InstituteId && !t.Enrollments!.Any(u => u.StudentId == student.Id))
                .ToListAsync();

            if (student.Thesis != null) 
            {
                ViewBag.Thesis = student.Thesis;  
            }

            return View(theses);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAvailableTheses(int studentId)
        {
            Student? student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments!)
                    .ThenInclude(e => e.Thesis)
                        .ThenInclude(t => t.Promoter)
                            .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == studentId);

            if (student != null)
            {
                List<int> thesisIds = student.Enrollments
                    .Where(e => e.Thesis != null)
                    .Select(e => e.Thesis!.Id)
                    .ToList();

                List<Thesis> theses = await _context.Theses
                    .Include(t => t.Promoter!)
                        .ThenInclude(p => p.User)
                    .Where(t => t.StudentId == null && t.Promoter!.User!.InstituteId == student.User!.InstituteId && !thesisIds.Contains(t.Id))
                    .ToListAsync();

                if (student.Enrollments.Any())
                {
                    ViewBag.Enrollments = student.Enrollments;
                }

                ViewBag.SelectedStudentId = studentId;

                return View(theses);
            }
            else
            {
                return NotFound("Student not found.");
            }
        }

        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> ActiveTheses()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                List<Thesis> theses = await _context.Theses
                    .Include(t => t.Student!)
                        .ThenInclude(p => p.User)
                    .Where(t => t.Promoter!.User!.Id == user.Id && t.StudentId != null)
                    .ToListAsync();

                return View(theses);
            }
            else 
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "SeminarLeader")]
        public async Task<IActionResult> SeminarLeaderTheses(string? currentFilter, string? searchString, int? page, string sortOrder = "")
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                ViewBag.CurrentSort = sortOrder;
                ViewBag.TitleSortParam = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
                ViewBag.StudentSortParam = sortOrder =="student" ? "student_desc" : "student";
                ViewBag.PromoterSortParam = sortOrder == "promoter" ? "promoter_desc" : "promoter";
                ViewBag.StatusSortParam = sortOrder == "status" ? "status_desc" : "status";

                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;


                IQueryable<Thesis> thesesQuery = _context.Theses
                    .Include(t => t.Student!)
                        .ThenInclude(s => s.User)
                    .Include(t => t.Promoter!)
                        .ThenInclude(p => p.User)
                    .Where(t => t.Promoter!.User!.InstituteId == user.InstituteId);

                bool isEnum = Enum.TryParse<ThesisStatus>(searchString, true, out var statusEnum);

                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    
                    thesesQuery = thesesQuery.Where(t =>
                        t.Title.Contains(searchString) ||
                        (t.Student != null && t.Student.User!.FirstName.Contains(searchString)) ||
                        (t.Student != null && t.Student.User!.LastName.Contains(searchString)) ||
                        t.Promoter!.User!.FirstName.Contains(searchString) ||
                        t.Promoter!.User!.LastName.Contains(searchString) ||
                        (isEnum == true && t.Status == statusEnum));
                }

                List<Thesis> theses = await thesesQuery.ToListAsync();

                var sortMapping = new Dictionary<string, Func<IEnumerable<Thesis>, IOrderedEnumerable<Thesis>>>
                {
                    { "title_desc", theses => theses.OrderByDescending(t => t.Title) },
                    { "student_desc", theses => theses.OrderByDescending(t => t.Student?.User!.LastName ?? string.Empty) },
                    { "student", theses => theses.OrderBy(t => t.Student?.User!.LastName ?? string.Empty) },
                    { "promoter", theses => theses.OrderBy(t => t.Promoter!.User!.LastName) },
                    { "promoter_desc", theses => theses.OrderByDescending(t => t.Promoter!.User!.LastName) },
                    { "status", theses => theses.OrderBy(t => t.Status.ToString()) },
                    { "status_desc", theses => theses.OrderByDescending(t => t.Status.ToString()) }
                };

                if (sortMapping.TryGetValue(sortOrder, out var sortFunc))
                {
                    theses = sortFunc(theses).ToList();
                }
                else
                {
                    theses = theses.OrderBy(t => t.Title).ToList();
                }

                int pageSize = 2;
                if (page < 1)
                {
                    page = 1;
                }

                int recordsCount = theses.Count();
                int pageNumber = page ?? 1;

                PagingService pager = new PagingService(recordsCount, pageNumber, pageSize);
                ViewBag.Pager = pager;

                int itemsToSkip = (pageNumber - 1) * pageSize;

                List<Thesis> items = theses
                    .Skip(itemsToSkip)
                    .Take(pageSize)
                    .ToList();
                return View(items);
            }
            else 
            {
                return NotFound();
            }
        }

        // GET: Thesis/Details/5
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDetails(int id)
        {
            Thesis? thesis = await _context.Theses
                .Include(t => t.PdfFiles)
                .Include(t => t.PresentationFile)
                .Include(t => t.Promoter!)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (thesis == null)
            {
                return NotFound();
            }
            else 
            {
                var viewModel = new PromoterThesisViewModel{
                    Id = id,
                    Title = thesis.Title,
                    Description = thesis.Description,
                    PromoterId = thesis.PromoterId,
                    Promoter = thesis.Promoter,
                };

                List<PdfFile> examplePdfs = thesis.PdfFiles?.Where(p => p.PdfType == PdfType.example).ToList() ?? [];
                if (examplePdfs.Any())
                {
                    viewModel.SystemFiles = examplePdfs;
                }

                ViewBag.OriginalPdf = thesis.PdfFiles?.FirstOrDefault(p => p.PdfType == PdfType.original);
                ViewBag.PresentationFile = thesis.PresentationFile;

                return View(viewModel);
            }
        }

        [Authorize(Roles = "Director, Promoter")]
        public async Task<IActionResult> PromoterDetails(int id, int previousActionType = 0)
        {
            Thesis? thesis = await _context.Theses
                .Include(t => t.Promoter!)
                    .ThenInclude(p => p.User)
                .Include(t => t.PdfFiles)
                .Include(t => t.PresentationFile)
                .Include(t => t.Enrollments!)
                    .ThenInclude(e => e.Student!)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (thesis == null)
            {
                return NotFound();
            }
            else
            {
                var viewModel = new PromoterThesisViewModel
                {
                    Id = id,
                    Title = thesis.Title,
                    Description = thesis.Description,
                    PromoterId = thesis.PromoterId,
                    Promoter = thesis.Promoter,
                    Comment = thesis.Comment,
                    ThesisSophistication = thesis.ThesisSophistication,
                    Enrollments = thesis.Enrollments
                };

                List<PdfFile> examplePdfs = thesis.PdfFiles?.Where(p => p.PdfType == PdfType.example).ToList() ?? [];
                if (examplePdfs.Any())
                {
                    viewModel.SystemFiles = examplePdfs;
                }

                ViewBag.OriginalPdf = thesis.PdfFiles?.FirstOrDefault(p => p.PdfType == PdfType.original);
                ViewBag.PresentationFile = thesis.PresentationFile;

                switch(previousActionType) 
                {
                    case 1:
                        ViewBag.PreviousRoute = "Thesis|SeminarLeaderTheses";
                        break;
                    default:
                        ViewBag.PreviousRoute = "Institute|InstituteAssignedStudents";
                        break;
                }

                return View(viewModel);
            }
        }

        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> ManageActiveThesis(int id)
        {
            Thesis? thesis = await _context.Theses
                .Include(t => t.PdfFiles)
                .Include(t => t.PresentationFile)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (thesis == null)
            {
                return NotFound();
            }
            else
            {
                var viewModel = new PromoterThesisViewModel
                {
                    Id = id,
                    Title = thesis.Title,
                    Description = thesis.Description,
                    PromoterId = thesis.PromoterId,
                    Comment = thesis.Comment,
                    ThesisSophistication = thesis.ThesisSophistication
                };

                ViewBag.OriginalPdf = thesis.PdfFiles?.FirstOrDefault(p => p.PdfType == PdfType.original);
                ViewBag.PresentationFile = thesis.PresentationFile;

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AddStudentFiles(PromoterThesisViewModel vm)
        {
            Thesis? thesis = await _context.Theses
                .FirstOrDefaultAsync(m => m.Id == vm.Id);

            if (vm.PdfFile != null)
            {
                var originalFileName = Path.GetFileNameWithoutExtension(vm.PdfFile.FileName);
                var fileExtension = Path.GetExtension(vm.PdfFile.FileName);
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"{originalFileName}_{timeStamp}{fileExtension}";
                var path = Path.Combine(_env.WebRootPath, _configuration.GetSection("FileManagement:SystemFileUploads").Value);
                var filePath = Path.Combine(path, fileName);

                var stream = new FileStream(filePath, FileMode.Create);
                await vm.PdfFile.CopyToAsync(stream);

                var uploadedFile = new PdfFile
                {
                    Uploaded = DateTime.Now,
                    FileType = vm.PdfFile.ContentType,
                    FileName = fileName,
                    PdfType = PdfType.original,
                    FilePath = filePath,
                    Extension = fileExtension,
                    Thesis = thesis,
                    FileStatus = FileStatus.NotVerified
                };

                await _context.PdfFiles.AddAsync(uploadedFile);
            }

            if (vm.PresentationFile != null)
            {
                var originalFileName = Path.GetFileNameWithoutExtension(vm.PresentationFile.FileName);
                var fileExtension = Path.GetExtension(vm.PresentationFile.FileName);
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"{originalFileName}_{timeStamp}{fileExtension}";
                var path = Path.Combine(_env.WebRootPath, _configuration.GetSection("FileManagement:SystemPresentationUploads").Value);
                var filePath = Path.Combine(path, fileName);

                var stream = new FileStream(filePath, FileMode.Create);
                await vm.PresentationFile.CopyToAsync(stream);

                var uploadedFile = new PresentationFile
                {
                    Uploaded = DateTime.Now,
                    FileType = vm.PresentationFile.ContentType,
                    FileName = fileName,
                    FilePath = filePath,
                    Extension = fileExtension,
                    Thesis = thesis,
                    FileStatus = FileStatus.NotVerified
                };

                await _context.PresentationFiles.AddAsync(uploadedFile);
            }

            await _context.SaveChangesAsync();

            _notificationService.AddNotification($"FilesAdded_{User.Identity.Name}", "Your files has been added to the system. Please, wait for promoter to check it.");
            return RedirectToAction(nameof(StudentDetails), new { id = vm.Id });
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
                var path = Path.Combine(_env.WebRootPath, _configuration.GetSection("FileManagement:SystemFileUploads").Value);
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
                    Thesis? thesis = await _context.Theses
                        .Include(p => p.PdfFiles)
                        .FirstOrDefaultAsync(m => m.Id == (int)vm.Id);

                    thesis.Title = vm.Title;
                    thesis.Description = vm.Description;

                    _context.Update(thesis);

                    if (vm.PdfFile != null)
                    {
                        var originalFileName = Path.GetFileNameWithoutExtension(vm.PdfFile.FileName);
                        var fileExtension = Path.GetExtension(vm.PdfFile.FileName);
                        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        var fileName = $"{originalFileName}_{timeStamp}{fileExtension}";
                        var path = Path.Combine(_env.WebRootPath, _configuration.GetSection("FileManagement:SystemFileUploads").Value);
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
                if (thesis.StudentId != null) {
                    _notificationService.AddNotification($"DeleteFailure_{User.Identity.Name}", "You cant delete thesis with an assigned student.");
                } 
                else 
                {
                    _context.Theses.Remove(thesis);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Files/DownloadDocument/5
        public async Task<IActionResult> DownloadDocument(int id)
        {
            return await DownloadFile(id, _context.PdfFiles);
        }

        // GET: /Files/DownloadPresentation/5
        public async Task<IActionResult> DownloadPresentation(int id)
        {
            return await DownloadFile(id, _context.PresentationFiles);
        }

        private async Task<IActionResult> DownloadFile<T>(int id, DbSet<T> dbSet) where T : class, IFile
        {
            T? file = await dbSet.FirstOrDefaultAsync(m => m.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            string filePath = file.FilePath;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, file.FileType, file.FileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> UpdateDocumentStatus(int id, string status)
        {
            return await UpdateFileSatus(id, status, _context.PdfFiles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> UpdatePresentationStatus(int id, string status)
        {
            return await UpdateFileSatus(id, status, _context.PresentationFiles);
        }

        private async Task<IActionResult> UpdateFileSatus<T>(int id, string status, DbSet<T> dbSet) where T : class, IFile
        {
            T? file = await dbSet.FirstOrDefaultAsync(m => m.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            switch (status)
            {
                case "Accepted":
                    file.FileStatus = FileStatus.Accepted;
                    break;
                case "NotAccepted":
                    file.FileStatus = FileStatus.NotAccepted;
                    break;
                case "NotVerified":
                    file.FileStatus = FileStatus.NotVerified;
                    break;
            }

            _context.Update(file);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Promoter")]
        public async Task<IActionResult> SetThesisConclusion(int id, string conclusion, int conclusionType)
        {
            Thesis? thesis = await _context.Theses.FirstOrDefaultAsync(m => m.Id == id);

            if (thesis == null)
            {
                return NotFound();
            } 
            else 
            {
                if (conclusionType == 0) 
                {
                    thesis.Comment = conclusion;
                } 
                else 
                {
                    thesis.ThesisSophistication = conclusion;
                }
                
                _context.Update(thesis);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = id });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignThesisToStudents(List<int> selectedStudents)
        {
            if (selectedStudents != null && selectedStudents.Any())
            {
                List<Student> students = await _context.Students
                    .Where(s => selectedStudents.Contains(s.Id))
                    .Include(s => s.User)
                    .Include(s => s.Enrollments!)
                        .ThenInclude(e => e.Thesis)
                    .ToListAsync();

                // Students with enrollments come first
                students = students
                    .OrderByDescending(s => s.Enrollments != null && s.Enrollments.Any())
                    .ToList();

                List<Thesis> thesesWithoutEnrollments = await _context.Theses
                    .Where(t => !t.Enrollments.Any())
                    .ToListAsync();

                var random = new Random();

                foreach (var student in students)
                {
                    if (student.Enrollments != null && student.Enrollments.Any())
                    {
                        // Get a random enrollment
                        List<Enrollment> enrollmentList = student.Enrollments.ToList();    
                        int randomIndex = random.Next(enrollmentList.Count);
                        Enrollment randomEnrollment = enrollmentList[randomIndex];

                        if (randomEnrollment != null)
                        {
                            randomEnrollment.Thesis.Status = ThesisStatus.InProgress;
                            randomEnrollment.Thesis.StudentId = student.Id;

                            List<Enrollment> thesisEnrollments = await _context.Enrollments
                                .Where(t => t.ThesisId == randomEnrollment.Thesis.Id && t.StudentId != student.Id)
                                .ToListAsync();

                            thesisEnrollments.AddRange(student.Enrollments);

                            await _thesisRepository.assignThesisToStudent(randomEnrollment.Thesis, student.Id, thesisEnrollments);
                        } 
                    }
                    else
                    {
                        if (thesesWithoutEnrollments.Any())
                        {
                            int randomIndex = random.Next(thesesWithoutEnrollments.Count);
                            Thesis randomThesis = thesesWithoutEnrollments[randomIndex];

                            List<Enrollment> thesisEnrollments = await _context.Theses
                                .Where(t => t.Id == randomThesis.Id)
                                .SelectMany(t => t.Enrollments)
                                .ToListAsync();

                            await _thesisRepository.assignThesisToStudent(randomThesis, student.Id, thesisEnrollments);
                        }
                        else
                        {
                            _notificationService.AddNotification($"AssignThesisError_{User.Identity.Name}", "There is no available theses right now, please try again later.");
                        }
                    }
                }

                _notificationService.AddNotification($"SuccessfullAssigned_{User.Identity.Name}", $"{students.Count} students have been successfully processed and assigned to available theses.");
            }

            // _notificationService.AddNotification($"ErrorMessage_{User.Identity.Name}", "No students were selected.");
            return RedirectToAction("StudentsWithoutThesis","Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignThesisToStudentManual(int selectedStudent, int thesisId)
        {
            Student? student = await _context.Students
                .Where(s => s.Id == selectedStudent)
                .Include(s => s.User)
                .Include(s => s.Enrollments!)
                    .ThenInclude(e => e.Thesis)
                .FirstOrDefaultAsync();

            if (student != null)
            {
                Thesis? thesis = await _context.Theses
                    .Where(t => t.Id == thesisId)
                    .Include(t => t.Enrollments)
                    .FirstOrDefaultAsync();
                if (thesis != null)
                {

                    List<Enrollment> thesisEnrollments = thesis.Enrollments.Where(e => e.StudentId != student.Id).ToList();
                    thesisEnrollments.AddRange(student.Enrollments);

                    await _thesisRepository.assignThesisToStudent(thesis, student.Id, thesisEnrollments);

                    _notificationService.AddNotification($"SuccessfullAssigned_{User.Identity.Name}", "Selected student have been successfully processed and assigned to available thesis.");
                }
                else
                {
                    _notificationService.AddNotification($"AssignThesisError_{User.Identity.Name}", "There is no available theses right now, please try again later.");
                }
            }

            // _notificationService.AddNotification($"AssignThesisError_{User.Identity.Name}", "No students were selected.");
            return RedirectToAction("StudentsWithoutThesis", "Student");
        }

        private bool ThesisExists(int id)
        {
            return _context.Theses.Any(e => e.Id == id);
        }
    }
}
