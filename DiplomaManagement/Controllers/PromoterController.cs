using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DiplomaManagement.Data;
using DiplomaManagement.Models;
using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Identity;
using DiplomaManagement.Interfaces;
using DiplomaManagement.Services;
using DiplomaManagement.Resources;
using Microsoft.Extensions.Localization;

namespace DiplomaManagement.Controllers
{
    public class PromoterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResource> _htmlLocalizer;


        public PromoterController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, INotificationService notificationService, IStringLocalizer<SharedResource> htmlLocalizer)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _notificationService = notificationService;
            _htmlLocalizer = htmlLocalizer;
        }

        // GET: Promoter
        public async Task<IActionResult> Index(string? currentFilter, string? searchString, int? page, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.SurnameSortParam = sortOrder == "surname" ? "surname_desc" : "surname";
            ViewBag.EmailSortParam = sortOrder == "email" ? "email_desc" : "email";
            ViewBag.ThesisLimitSortParam = sortOrder == "thesis_limit" ? "thesis_limit_desc" : "thesis_limit";
            ViewBag.DirectorSortParam = sortOrder == "director" ? "director_desc" : "director";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            IQueryable<Promoter> promotersQuery = _context.Promoters
                .Include(p => p.Director.User)
                .Include(p => p.User);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                promotersQuery = promotersQuery.Where(t =>
                    t.User.FirstName.Contains(searchString) ||
                    t.User.LastName.Contains(searchString) ||
                    t.User.Email.Contains(searchString) ||
                    t.ThesisLimit.ToString().Contains(searchString) ||
                    t.Director.User.FirstName.Contains(searchString) ||
                    t.Director.User.LastName.Contains(searchString));
            }

            List<Promoter> promoters = await promotersQuery.ToListAsync();

            var sortMapping = new Dictionary<string, Func<IEnumerable<Promoter>, IOrderedEnumerable<Promoter>>>
            {
                { "name_desc", theses => theses.OrderByDescending(t => t.User.FirstName) },
                { "surname_desc", theses => theses.OrderByDescending(t => t.User.LastName) },
                { "surname", theses => theses.OrderBy(t => t.User.LastName) },
                { "email", theses => theses.OrderBy(t => t.User.Email) },
                { "email_desc", theses => theses.OrderByDescending(t => t.User.Email) },
                { "thesis_limit", theses => theses.OrderBy(t => t.ThesisLimit) },
                { "thesis_limit_desc", theses => theses.OrderByDescending(t => t.ThesisLimit) },
                { "director", theses => theses.OrderBy(t => t.Director.User.FirstName) },
                { "director_desc", theses => theses.OrderByDescending(t => t.Director.User.FirstName) }
            };

            if (sortMapping.TryGetValue(sortOrder ?? "", out var sortFunc))
            {
                promoters = sortFunc(promoters).ToList();
            }
            else
            {
                promoters = promoters.OrderBy(t => t.User.FirstName).ToList();
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

            return View(items);
        }

        // GET: Promoter/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Promoter? promoter = await _context.Promoters
                .Include(p => p.Director.User)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (promoter == null)
            {
                return NotFound();
            }

/*            var detailsViewModel = new DetailsViewModel<Promoter>
            {
                Entity = promoter,
                ResetPasswordViewModel = new ResetPasswordViewModel
                {
                    Id = promoter.Id
                }
            };*/

            return View(promoter);
        }

        // GET: Promoter/Create
        public IActionResult Create()
        {
            List<Director> directorUsers = _context.Directors
                .Include(d => d.User)
                    .ThenInclude(u => u.Institute)
                .ToList();

            List<SelectListItem> directorsSelectList = directorUsers.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.User.FirstName} {u.User.LastName} ({u.User.Institute.Name})"
            }).ToList();

            ViewBag.Directors = new SelectList(directorsSelectList, "Value", "Text");
            return View();
        }

        // POST: Promoter/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromoterViewModel promoterViewModel)
        {
            if (ModelState.IsValid)
            {
                Director? director = await _context.Directors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == promoterViewModel.DirectorId);

                ApplicationUser user = new ApplicationUser
                {
                    UserName = promoterViewModel.Email,
                    Email = promoterViewModel.Email,
                    FirstName = promoterViewModel.FirstName,
                    LastName = promoterViewModel.LastName,
                    EmailConfirmed = true,
                    InstituteId = director.User.InstituteId
                };

                var result = await _userManager.CreateAsync(user, promoterViewModel.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Promoter");

                    if (promoterViewModel.isSeminarLeader)
                    {
                        await _userManager.AddToRoleAsync(user, "SeminarLeader");
                    }

                    Promoter promoter = new Promoter
                    {
                        PromoterUserId = user.Id,
                        DirectorId = promoterViewModel.DirectorId
                    };

                    _context.Add(promoter);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            List<Director> directorUsers = _context.Directors
               .Include(d => d.User)
                   .ThenInclude(u => u.Institute)
               .ToList();

            List<SelectListItem> directorsSelectList = directorUsers.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.User.FirstName} {u.User.LastName} ({u.User.Institute.Name})"
            }).ToList();

            ViewBag.Directors = new SelectList(directorsSelectList, "Value", "Text");
            return View(promoterViewModel);
        }

        // GET: Promoter/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Promoter? promoter = await _context.Promoters
                .Include(d => d.User)
                .Include(d => d.Director)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (promoter == null)
            {
                return NotFound();
            }

            PromoterViewModel promoterViewModel = new PromoterViewModel
            {
                Id = promoter.Id,
                FirstName = promoter.User.FirstName,
                LastName = promoter.User.LastName,
                Email = promoter.User.Email,
                ThesisLimit = promoter.ThesisLimit,
                DirectorId = promoter.DirectorId
            };

            if (await _userManager.IsInRoleAsync(promoter.User, "SeminarLeader"))
            {
                promoterViewModel.isSeminarLeader = true;
            }
            else
            {
                promoterViewModel.isSeminarLeader = false;
            }

            DetailsViewModel<PromoterViewModel> detailsViewModel = new DetailsViewModel<PromoterViewModel>
            {
                Entity = promoterViewModel,
                ResetPasswordViewModel = new ResetPasswordViewModel
                {
                    Id = promoter.Id
                }
            };

            List<Director> directorUsers = _context.Directors
                .Include(d => d.User)
                    .ThenInclude(u => u.Institute)
                .ToList();

            List<SelectListItem> directorsSelectList = directorUsers.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.User.FirstName} {u.User.LastName} ({u.User.Institute.Name})"
            }).ToList();

            ViewBag.Directors = new SelectList(directorsSelectList, "Value", "Text");
            return View(detailsViewModel);
        }

        // POST: Promoter/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DetailsViewModel<PromoterViewModel> detailsViewModel)
        {
            if (id != detailsViewModel.Entity.Id)
            {
                return NotFound();
            }

            try
            {
                var existingPromoter = await _context.Promoters
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (existingPromoter == null)
                {
                    return NotFound();
                }

                existingPromoter.ThesisLimit = detailsViewModel.Entity.ThesisLimit ?? 0;
                existingPromoter.User.FirstName = detailsViewModel.Entity.FirstName;
                existingPromoter.User.LastName = detailsViewModel.Entity.LastName;
                existingPromoter.User.Email = detailsViewModel.Entity.Email;
                existingPromoter.DirectorId = detailsViewModel.Entity.DirectorId;

                if (detailsViewModel.Entity.isSeminarLeader)
                {
                    await _userManager.AddToRoleAsync(existingPromoter.User, "SeminarLeader");
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(existingPromoter.User, "SeminarLeader");
                }

                _context.Update(existingPromoter.User);
                _context.Update(existingPromoter);
                await _context.SaveChangesAsync();
            }
            // basicaly it's not nedded (only if we have more that one administrator)
            catch (DbUpdateConcurrencyException)
            {
                if (!PromoterExists(detailsViewModel.Entity.Id))
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

        // POST: Promoter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promoter = await _context.Promoters
                .Include(p => p.User)
                .Include(p => p.Theses)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (promoter != null)
            {
                if (promoter.Theses.Any())
                {
                    _notificationService.AddNotification($"ErrorMessage_{User.Identity!.Name}", _htmlLocalizer["promoter-delete-error"]);
                }
                else
                {
                    var user = promoter.User;

                    if (user != null)
                    {
                        _context.Promoters.Remove(promoter);
                        _context.Users.Remove(user);
                        await _context.SaveChangesAsync();
                        _notificationService.AddNotification($"PromoterDeleted_{User.Identity!.Name}", _htmlLocalizer["promoter-deleted-message", user.FirstName, user.LastName]);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(DetailsViewModel<Promoter> model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id = model.ResetPasswordViewModel.Id });
            }

            Promoter? promoter = await _context.Promoters
                .Include(p => p.Director.User)
                .Include(p => p.User)
                .FirstOrDefaultAsync(d => d.Id == model.ResetPasswordViewModel.Id);

            if (promoter != null)
            {
                var user = promoter.User;

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var result = await _userManager.ResetPasswordAsync(user, token, model.ResetPasswordViewModel.NewPassword);

                if (result.Succeeded)
                {
                    _notificationService.AddNotification($"SuccessMessage_{User.Identity.Name}", "User password has been successfully reset.");
                    return RedirectToAction(nameof(Edit), new { id = promoter.Id });
                }
                else
                {
                    model.Entity = promoter;
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View("Edit", model);
                }
            }
            else
            {
                return RedirectToAction(nameof(Edit), new { id = model.ResetPasswordViewModel.Id });
            }
        }

        private bool PromoterExists(int id)
        {
            return _context.Promoters.Any(e => e.Id == id);
        }
    }
}
