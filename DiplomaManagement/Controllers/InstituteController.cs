﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Authorization;
using DiplomaManagement.Services;
using DiplomaManagement.Interfaces;
using Microsoft.AspNetCore.Identity;
using DiplomaManagement.Resources;
using Microsoft.Extensions.Localization;

namespace DiplomaManagement.Controllers
{
    public class InstituteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResource> _htmlLocalizer;

        public InstituteController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService, IStringLocalizer<SharedResource> htmlLocalizer)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _htmlLocalizer = htmlLocalizer;
        }

        // GET: Institute
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? currentFilter, string? searchString, int? page, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.SiteSortParam = sortOrder == "site" ? "site_desc" : "site";
            ViewBag.StreetSortParam = sortOrder == "street" ? "street_desc" : "street";
            ViewBag.EmailSortParam = sortOrder == "email" ? "email_desc" : "email";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            IQueryable<Institute> instituteQuery = _context.Institutes;

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                instituteQuery = instituteQuery.Where(t =>
                    t.Name.Contains(searchString) ||
                    t.SiteAddress.Contains(searchString) ||
                    t.Street.Contains(searchString) ||
                    t.Email.Contains(searchString));
            }

            List<Institute> institutes = await instituteQuery.ToListAsync();

            var sortMapping = new Dictionary<string, Func<IEnumerable<Institute>, IOrderedEnumerable<Institute>>>
            {
                { "name_desc", theses => theses.OrderByDescending(t => t.Name) },
                { "site_desc", theses => theses.OrderByDescending(t => t.SiteAddress) },
                { "site", theses => theses.OrderBy(t => t.SiteAddress) },
                { "street", theses => theses.OrderBy(t => t.Street) },
                { "street_desc", theses => theses.OrderByDescending(t => t.Street) },
                { "email", theses => theses.OrderBy(t => t.Email) },
                { "email_desc", theses => theses.OrderByDescending(t => t.Email) }
            };

            if (sortMapping.TryGetValue(sortOrder ?? "", out var sortFunc))
            {
                institutes = sortFunc(institutes).ToList();
            }
            else
            {
                institutes = institutes.OrderBy(t => t.Name).ToList();
            }

            int pageSize = 10;
            if (page < 1)
            {
                page = 1;
            }

            int recordsCount = institutes.Count();
            int pageNumber = page ?? 1;

            PagingService pager = new PagingService(recordsCount, pageNumber, pageSize);
            ViewBag.Pager = pager;

            int itemsToSkip = (pageNumber - 1) * pageSize;

            List<Institute> items = institutes
                .Skip(itemsToSkip)
                .Take(pageSize)
                .ToList();

            return View(items);
        }

        [Authorize(Roles = "Director, SeminarLeader")]
        public async Task<IActionResult> InstituteAssignedStudents(string? currentFilter, string? searchString, int? page, string sortOrder)
        {
            var user = await _userManager.GetUserAsync(User);

            var director = await _context.Directors
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.DirectorUserId == user.Id);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParam = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.StudentSortParam = sortOrder == "student" ? "student_desc" : "student";
            ViewBag.EmailSortParam = sortOrder == "email" ? "email_desc" : "email";
            ViewBag.ThesisTitleSortParam = sortOrder == "thesis_title" ? "thesis_title_desc" : "thesis_title";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            IQueryable<Student> studentsQuery = _context.Students
                .Include(s => s.User)
                .Include(s => s.Thesis)
                .Where(i => i.User.InstituteId == director.User.InstituteId);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                studentsQuery = studentsQuery.Where(t =>
                    t.Id.ToString().Contains(searchString) ||
                    t.User.FirstName.Contains(searchString) ||
                    t.User.LastName.Contains(searchString) ||
                    t.User.Email.Contains(searchString) ||
                    t.Thesis != null && t.Thesis.Title.Contains(searchString));
            }

            List<Student> students = await studentsQuery.ToListAsync();

            var sortMapping = new Dictionary<string, Func<IEnumerable<Student>, IOrderedEnumerable<Student>>>
            {
                { "id_desc", theses => theses.OrderByDescending(t => t.Id) },
                { "student", theses => theses.OrderBy(t => t.User.FirstName) },
                { "student_desc", theses => theses.OrderByDescending(t => t.User.FirstName) },
                { "email", theses => theses.OrderBy(t => t.User.Email) },
                { "email_desc", theses => theses.OrderByDescending(t => t.User.Email) },
                { "thesis_title", theses => theses.OrderBy(t => t.Thesis?.Title ?? string.Empty) },
                { "thesis_title_desc", theses => theses.OrderByDescending(t => t.Thesis?.Title ?? string.Empty) }
            };

            if (sortMapping.TryGetValue(sortOrder ?? "", out var sortFunc))
            {
                students = sortFunc(students).ToList();
            }
            else
            {
                students = students.OrderBy(t => t.Id).ToList();
            }

            int pageSize = 10;
            if (page < 1)
            {
                page = 1;
            }

            int recordsCount = students.Count();
            int pageNumber = page ?? 1;

            PagingService pager = new PagingService(recordsCount, pageNumber, pageSize);
            ViewBag.Pager = pager;

            int itemsToSkip = (pageNumber - 1) * pageSize;

            List<Student> items = students
                .Skip(itemsToSkip)
                .Take(pageSize)
                .ToList();

            return View(students);
        }

        // GET: Institute/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var institute = await _context.Institutes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (institute == null)
            {
                return NotFound();
            }

            return View(institute);
        }

        // GET: Institute/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Institute/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,SiteAddress,Street,City,PostalCode,Email")] Institute institute)
        {
            if (ModelState.IsValid)
            {
                _context.Add(institute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(institute);
        }

        // GET: Institute/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var institute = await _context.Institutes.FindAsync(id);
            if (institute == null)
            {
                return NotFound();
            }
            return View(institute);
        }

        // POST: Institute/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SiteAddress,Street,City,PostalCode,Email")] Institute institute)
        {
            if (id != institute.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(institute);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstituteExists(institute.Id))
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
            return View(institute);
        }

        // POST: Institute/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var institute = await _context.Institutes
                .Include(i => i.Users)
                    .ThenInclude(u => u.UserDirector)
                .Include(i => i.Users)
                    .ThenInclude(u => u.UserPromoter)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (institute == null)
            {
                return NotFound();
            }

            if (institute.Users.Any())
            {
                if (institute.Users.Any(u => u.UserPromoter != null) || institute.Users.Any(u => u.UserDirector != null))
                {
                    _notificationService.AddNotification($"ErrorMessage_{User.Identity!.Name}", _htmlLocalizer["institute-delete-error"]);
                }
            } 
            else 
            {
                _context.Institutes.Remove(institute);
                await _context.SaveChangesAsync();
                _notificationService.AddNotification($"InstituteDeleted_{User.Identity!.Name}", _htmlLocalizer["institute-deleted-message", institute.Name]);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Statistics()
        {
            var user = await _userManager.GetUserAsync(User);

            Director? director = await _context.Directors
                .Include(d => d.Promoters!)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.DirectorUserId == user.Id);

            if (director != null) 
            {
                List<Thesis> theses = await _context.Theses
                    .Where(t => director.Promoters!.Contains(t.Promoter))
                    .ToListAsync();

                var thesesData = new Dictionary<string, int>();

                int availableCount = theses.Count(t => t.Status == ThesisStatus.Available);
                int requestedCount = theses.Count(t => t.Status == ThesisStatus.Requested);
                int inProgressCount = theses.Count(t => t.Status == ThesisStatus.InProgress);
                int acceptedCount = theses.Count(t => t.Status == ThesisStatus.Accepted);

                if (availableCount > 0)
                {
                    thesesData.Add("Available", availableCount);
                }

                if (requestedCount > 0)
                {
                    thesesData.Add("Requested", requestedCount);
                }

                if (inProgressCount > 0)
                {
                    thesesData.Add("In Progress", inProgressCount);
                }

                if (acceptedCount > 0)
                {
                    thesesData.Add("Accepted", acceptedCount);
                }

                var promotersData = new Dictionary<string, int>();

                foreach (var promoter in director.Promoters)
                {
                    int thesisCount = theses.Count(t => t.Promoter == promoter);
                    promotersData.Add(promoter.User!.FirstName + " " + promoter.User.LastName, thesisCount);
                }

                ViewBag.ThesesData = thesesData;
                ViewBag.PromotersData = promotersData;

                var promotersAcceptedTheses = new Dictionary<string, int>();

                foreach (var promoter in director.Promoters)
                {
                    int thesisCount = theses.Count(t => t.Promoter == promoter && t.Status == ThesisStatus.Accepted);
                    promotersAcceptedTheses.Add(promoter.User!.FirstName + " " + promoter.User.LastName, thesisCount);
                }

                ViewBag.PromotersAcceptedTheses = promotersAcceptedTheses;

                return View();
            }
            else
            {
                return NotFound();
            }
        }

        private bool InstituteExists(int id)
        {
            return _context.Institutes.Any(e => e.Id == id);
        }
    }
}
