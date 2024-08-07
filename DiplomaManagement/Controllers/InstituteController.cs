using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Authorization;
using DiplomaManagement.Services;
using DiplomaManagement.Interfaces;

namespace DiplomaManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InstituteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public InstituteController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Institute
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

        // GET: Institute/Details/5
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

        // GET: Institute/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Institute/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
                string errorMessage = "It is not possible to delete this institute as there is linked data. Check ";

                if (institute.Users.Any(u => u.UserPromoter != null))
                {
                    errorMessage += "'Promoters', ";
                }

                if (institute.Users.Any(u => u.UserDirector != null))
                {
                    errorMessage += "'Directors' table.";
                }
                _notificationService.AddNotification($"ErrorMessage_{User.Identity.Name}", errorMessage);

                return RedirectToAction(nameof(Index));
            }

            _context.Institutes.Remove(institute);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InstituteExists(int id)
        {
            return _context.Institutes.Any(e => e.Id == id);
        }
    }
}
