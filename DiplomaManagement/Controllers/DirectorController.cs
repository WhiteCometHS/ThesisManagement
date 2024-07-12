using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using DiplomaManagement.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using DiplomaManagement.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace DiplomaManagement.Controllers
{
    public class DirectorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly InstituteRepository _instituteRepository;

        public DirectorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, InstituteRepository instituteRepository)
        {
            _context = context;
            _userManager = userManager;
            _instituteRepository = instituteRepository;
        }

        // GET: Director
        public async Task<IActionResult> Index()
        {
            List<DirectorViewModel> directors = await _context.Directors
                .Include(d => d.User)
                .ThenInclude(u => u.Institute)
                .Select(d => new DirectorViewModel
                {
                    Id = d.Id,
                    FirstName = d.User.FirstName,
                    LastName = d.User.LastName,
                    Email = d.User.Email,
                    Institute = d.User.Institute
                })
                .ToListAsync();

            return View(directors);
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
                // basicaly it's not nedded (only if we have more that one administrator)
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

        // GET: Director/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Director? director = await _context.Directors
                .Include(d => d.User)
                .ThenInclude(u => u.Institute)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (director == null)
            {
                return NotFound();
            }

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
                    TempData[$"ErrorMessage_{User.Identity.Name}"] = "It is not possible to delete this director as there is linked data. Check 'Promoters' table.";
                    return RedirectToAction(nameof(Index));
                }

                var user = director.User;

                _context.Directors.Remove(director);

                if (user != null)
                {
                    _context.Users.Remove(user);
                }

                await _context.SaveChangesAsync();
            }     

            return RedirectToAction(nameof(Index));
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
                    TempData[$"SuccessMessage_{User.Identity.Name}"] = "The password has been successfully reset.";
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
                TempData[$"ErrorMessage_{User.Identity.Name}"] = "Promoter not found.";
            }

            return RedirectToAction(nameof(Edit), new { id = model.ResetPasswordViewModel.Id });
        }

        [Authorize(Roles = "Director")]
        public async Task<IActionResult> AssignedPromoters()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            Director? director = await _context.Directors
                .Include(d => d.Promoters)
                .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(d => d.DirectorUserId == user.Id);

            return View(director);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> GlobalUpdateThesisLimit(int directorId, int thesisLimit)
        {
            if (thesisLimit == null)
            {
                return RedirectToAction(nameof(AssignedPromoters), new { id = directorId });
            }
            else
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
                }
            }

            return RedirectToAction(nameof(AssignedPromoters), new { id = directorId });
        }

        private bool DirectorExists(int id)
        {
            return _context.Directors.Any(e => e.Id == id);
        }
    }
}
