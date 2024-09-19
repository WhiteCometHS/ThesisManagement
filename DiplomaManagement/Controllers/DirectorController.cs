using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using DiplomaManagement.Models;
using Microsoft.AspNetCore.Identity;
using DiplomaManagement.Repositories;
using Microsoft.AspNetCore.Authorization;
using DiplomaManagement.Interfaces;
using DiplomaManagement.Services;
using Microsoft.Extensions.Localization;
using DiplomaManagement.Resources;

namespace DiplomaManagement.Controllers
{
    public class DirectorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly InstituteRepository _instituteRepository;
        private readonly IStringLocalizer<SharedResource> _htmlLocalizer;

        public DirectorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, InstituteRepository instituteRepository, INotificationService notificationService, IStringLocalizer<SharedResource> htmlLocalizer)
        {
            _context = context;
            _userManager = userManager;
            _instituteRepository = instituteRepository;
            _notificationService = notificationService;
            _htmlLocalizer = htmlLocalizer;
        }

        // GET: Director
        public async Task<IActionResult> Index(string? currentFilter, string? searchString, int? page, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.SurnameSortParam = sortOrder == "surname" ? "surname_desc" : "surname";
            ViewBag.EmailSortParam = sortOrder == "email" ? "email_desc" : "email";
            ViewBag.InstituteSortParam = sortOrder == "institute" ? "institute_desc" : "institute";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            IQueryable<DirectorViewModel> directorQuery = _context.Directors
                .Include(d => d.User)
                    .ThenInclude(u => u.Institute)
                .Select(d => new DirectorViewModel
                {
                    Id = d.Id,
                    FirstName = d.User.FirstName,
                    LastName = d.User.LastName,
                    Email = d.User.Email,
                    Institute = d.User.Institute
                });

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                directorQuery = directorQuery.Where(t =>
                    t.FirstName.Contains(searchString) ||
                    t.LastName.Contains(searchString) ||
                    t.Email.Contains(searchString) ||
                    t.Institute.Name.Contains(searchString));
            }

            List<DirectorViewModel> directors = await directorQuery.ToListAsync();

            var sortMapping = new Dictionary<string, Func<IEnumerable<DirectorViewModel>, IOrderedEnumerable<DirectorViewModel>>>
            {
                { "name_desc", theses => theses.OrderByDescending(t => t.FirstName) },
                { "surname_desc", theses => theses.OrderByDescending(t => t.LastName) },
                { "surname", theses => theses.OrderBy(t => t.LastName) },
                { "email", theses => theses.OrderBy(t => t.Email) },
                { "email_desc", theses => theses.OrderByDescending(t => t.Email) },
                { "institute", theses => theses.OrderBy(t => t.Institute.Name) },
                { "institute_desc", theses => theses.OrderByDescending(t => t.Institute.Name) }
            };

            if (sortMapping.TryGetValue(sortOrder ?? "", out var sortFunc))
            {
                directors = sortFunc(directors).ToList();
            }
            else
            {
                directors = directors.OrderBy(t => t.FirstName).ToList();
            }

            int pageSize = 10;
            if (page < 1)
            {
                page = 1;
            }

            int recordsCount = directors.Count();
            int pageNumber = page ?? 1;

            PagingService pager = new PagingService(recordsCount, pageNumber, pageSize);
            ViewBag.Pager = pager;

            int itemsToSkip = (pageNumber - 1) * pageSize;

            List<DirectorViewModel> items = directors
                .Skip(itemsToSkip)
                .Take(pageSize)
                .ToList();

            return View(items);
        }

        // GET: Director/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Director? director = await _context.Directors
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (director == null)
            {
                return NotFound();
            }

            return View(director);
        }

        // GET: Director/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            List<Institute> institutes = await _instituteRepository.GetInstitutesWithoutDirector();
            ViewBag.Institutes = new SelectList(institutes, "Id", "Name");
            return View();
        }

        // POST: Director/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DirectorViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    InstituteId = model.InstituteId
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Director");

                    Director director = new Director
                    {
                        DirectorUserId = user.Id
                    };

                    _context.Add(director);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            List<Institute> institutes = await _instituteRepository.GetInstitutesWithoutDirector();
            ViewBag.Institutes = new SelectList(institutes, "Id", "Name");
            return View(model);
        }

        // GET: Director/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Director? director = await _context.Directors
                .Include(d => d.User)
                .ThenInclude(u => u.Institute)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (director == null)
            {
                return NotFound();
            }

            DetailsViewModel<Director> detailsViewModel = new DetailsViewModel<Director>
            {
                Entity = director,
                ResetPasswordViewModel = new ResetPasswordViewModel
                {
                    Id = director.Id
                }
            };

            List<Institute> institutes = await _instituteRepository.GetInstitutesWithoutDirector();
            institutes.Add(director.User.Institute);

            ViewBag.Institutes = new SelectList(institutes, "Id", "Name");
            return View(detailsViewModel);
        }

        // POST: Director/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Director director)
        {
            if (id != director.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Director? existingDirector = await _context.Directors
                        .Include(d => d.User)
                        .FirstOrDefaultAsync(d => d.Id == id);

                    if (existingDirector == null)
                    {
                        return NotFound();
                    }

                    existingDirector.User.FirstName = director.User.FirstName;
                    existingDirector.User.LastName = director.User.LastName;
                    existingDirector.User.Email = director.User.Email;
                    existingDirector.User.InstituteId = director.User.InstituteId;

                    _context.Update(existingDirector.User);
                    _context.Update(existingDirector);
                    await _context.SaveChangesAsync();
                }
                // basicaly it's not nedded (only if we have more than one administrator)
                catch (DbUpdateConcurrencyException)
                {
                    if (!DirectorExists(director.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            List<Institute> institutes = await _instituteRepository.GetInstitutesWithoutDirector();
            institutes.Add(director.User.Institute);
            ViewBag.Institutes = new SelectList(institutes, "Id", "Name");
            return View(director);
        }

        // POST: Director/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Director? director = await _context.Directors
                .Include (d => d.Promoters)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (director == null)
            {
                return NotFound();
            }
            else
            {
                if (director.Promoters.Any())
                {
                    _notificationService.AddNotification($"ErrorMessage_{User.Identity!.Name}", _htmlLocalizer["director-delete-error"]);
                }
                else 
                {
                    var user = director.User;
                    _context.Directors.Remove(director);

                    if (user != null)
                    {
                        _context.Users.Remove(user);
                    }

                    await _context.SaveChangesAsync();
                    _notificationService.AddNotification($"DirectorDeleted_{User.Identity!.Name}", _htmlLocalizer["director-deleted-message", user.FirstName, user.LastName]);
                }

                return RedirectToAction(nameof(Index));
            }      
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(DetailsViewModel<Director> model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id = model.ResetPasswordViewModel.Id });
            }

            Director? director = await _context.Directors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == model.ResetPasswordViewModel.Id);

            if (director != null)
            {
                var user = director.User;

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var result = await _userManager.ResetPasswordAsync(user, token, model.ResetPasswordViewModel.NewPassword);

                if (result.Succeeded)
                {
                    _notificationService.AddNotification($"PasswordResetSuccess_{User.Identity.Name}", _htmlLocalizer["reset-password-success"]);
                    return RedirectToAction(nameof(Edit), new { id = director.Id });
                }
                else
                {
                    model.Entity = director;
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    ViewData["Institutes"] = new SelectList(_context.Institutes, "Id", "Name");
                    return View("Edit", model);
                }
            }
            else
            {
                return NotFound("Promoter not found.");
            }
        }

        [Authorize(Roles = "Director")]
        public async Task<IActionResult> AssignedPromoters(string? currentFilter, string? searchString, int? page, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParam = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.PromoterSortParam = sortOrder == "promoter" ? "promoter_desc" : "promoter";
            ViewBag.EmailSortParam = sortOrder == "email" ? "email_desc" : "email";
            ViewBag.ThesisLimitSortParam = sortOrder == "thesis_limit" ? "thesis_limit_desc" : "thesis_limit";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            int directorId = await _context.Directors
                .Where(d => d.User.Id == user.Id)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            IQueryable<Promoter> promotersQuery = _context.Promoters
                .Include(p => p.User)
                .Where(p => p.DirectorId == directorId);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                promotersQuery = promotersQuery.Where(t =>
                    t.Id.ToString().Contains(searchString) ||
                    t.User.FirstName.Contains(searchString) ||
                    t.User.LastName.Contains(searchString) ||
                    t.User.Email.Contains(searchString) ||
                    t.ThesisLimit.ToString().Contains(searchString));
            }

            List<Promoter> promoters = await promotersQuery.ToListAsync();

            var sortMapping = new Dictionary<string, Func<IEnumerable<Promoter>, IOrderedEnumerable<Promoter>>>
            {
                { "id_desc", theses => theses.OrderByDescending(t => t.Id) },
                { "name_desc", theses => theses.OrderByDescending(t => t.User.FirstName) },
                { "name", theses => theses.OrderBy(t => t.User.FirstName) },
                { "surname_desc", theses => theses.OrderByDescending(t => t.User.LastName) },
                { "surname", theses => theses.OrderBy(t => t.User.LastName) },
                { "email", theses => theses.OrderBy(t => t.User.Email) },
                { "email_desc", theses => theses.OrderByDescending(t => t.User.Email) },
                { "thesis_limit", theses => theses.OrderBy(t => t.ThesisLimit) },
                { "thesis_limit_desc", theses => theses.OrderByDescending(t => t.ThesisLimit) }
            };

            if (sortMapping.TryGetValue(sortOrder ?? "", out var sortFunc))
            {
                promoters = sortFunc(promoters).ToList();
            }
            else
            {
                promoters = promoters.OrderBy(t => t.Id).ToList();
            }

            int pageSize = 10;
            if (page < 1)
            {
                page = 1;
            }

            int recordsCount = promoters.Count();
            int pageNumber = page ?? 1;

            PagingService pager = new PagingService(recordsCount, pageNumber, pageSize);
            ViewBag.Pager = pager;

            int itemsToSkip = (pageNumber - 1) * pageSize;

            List<Promoter> items = promoters
                .Skip(itemsToSkip)
                .Take(pageSize)
                .ToList();

            ViewBag.DirectorId = directorId;

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> UpdatePromoterThesisLimit(int promoterId, int thesisLimit)
        {
            var promoter = await _context.Promoters.FindAsync(promoterId);
            if (promoter == null)
            {
                return NotFound();
            }

            promoter.ThesisLimit = thesisLimit;
            _context.Update(promoter);
            await _context.SaveChangesAsync();
            _notificationService.AddNotification($"SuccessMessage_{User.Identity!.Name}", _htmlLocalizer["thesis-limit-change-success", promoter.Id]);

            return RedirectToAction(nameof(AssignedPromoters));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> GlobalUpdateThesisLimit(int directorId, int thesisLimit)
        {
            Director? director = await _context.Directors
                .Include(d => d.Promoters)
                .FirstOrDefaultAsync(d => d.Id == directorId);

            if (director != null)
            {
                foreach (var promoter in director.Promoters)
                {
                    promoter.ThesisLimit = thesisLimit;
                }

                await _context.SaveChangesAsync();
                _notificationService.AddNotification($"SuccessMessage_{User.Identity!.Name}", _htmlLocalizer["thesis-limit-global-change-success"]);
            }

            return RedirectToAction(nameof(AssignedPromoters));
        }

        private bool DirectorExists(int id)
        {
            return _context.Directors.Any(e => e.Id == id);
        }
    }
}
