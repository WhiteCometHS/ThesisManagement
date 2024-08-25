using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using DiplomaManagement.Interfaces;
using DiplomaManagement.Models;
using DiplomaManagement.Repositories;
using DiplomaManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DiplomaManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? currentFilter, string? searchString, int? page, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.SurnameSortParam = sortOrder == "surname" ? "surname_desc" : "surname";
            ViewBag.EmailSortParam = sortOrder == "email" ? "email_desc" : "email";
            ViewBag.ThesisSortParam = sortOrder == "thesis" ? "thesis_desc" : "thesis";

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
                .Include(s => s.User!)
                    .ThenInclude(u => u.Institute)
                .Include(s => s.Thesis);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                studentsQuery = studentsQuery.Where(s =>
                    s.User.FirstName.Contains(searchString) ||
                    s.User.LastName.Contains(searchString) ||
                    s.User.Email.Contains(searchString) ||
                    s.Thesis.Title.Contains(searchString));
            }

            List<Student> students = await studentsQuery.ToListAsync();

            var sortMapping = new Dictionary<string, Func<IEnumerable<Student>, IOrderedEnumerable<Student>>>
            {
                { "name_desc", list => list.OrderByDescending(l => l.User.FirstName) },
                { "surname_desc", list => list.OrderByDescending(l => l.User.LastName) },
                { "surname", list => list.OrderBy(l => l.User.LastName) },
                { "email", list => list.OrderBy(l => l.User.Email) },
                { "email_desc", list => list.OrderByDescending(l => l.User.Email) },
                { "thesis", list => list.OrderBy(l => l.Thesis?.Title  ?? string.Empty) },
                { "thesis_desc", list => list.OrderByDescending(l => l.Thesis?.Title  ?? string.Empty) }
            };

            if (sortMapping.TryGetValue(sortOrder ?? "", out var sortFunc))
            {
                students = sortFunc(students).ToList();
            }
            else
            {
                students = students.OrderBy(s => s.User.FirstName).ToList();
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

            return View(items);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StudentsWithoutThesis(int? page)
        {
            List<Student> students = await _context.Students
                .Include(s => s.User!)
                    .ThenInclude(u => u.Institute)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Thesis)
                .Where(s => s.Thesis == null)
                .ToListAsync();

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

            return View(items);
        }
    }
}
