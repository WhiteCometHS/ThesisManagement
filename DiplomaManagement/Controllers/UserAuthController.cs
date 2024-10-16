using DiplomaManagement.Data;
using DiplomaManagement.Entities;
using DiplomaManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiplomaManagement.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace DiplomaManagement.Controllers
{
    public class UserAuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly InstituteRepository _instituteRepository;
        private readonly ApplicationDbContext _context;

        public UserAuthController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, InstituteRepository instituteRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _instituteRepository = instituteRepository;
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            loginModel.LoginInValid = "true";

            if (ModelState.IsValid)
            {
                ApplicationUser? signedUser = await _userManager.FindByEmailAsync(loginModel.Email);

                if (signedUser != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result;

                    if (signedUser.UserName != signedUser.Email)
                    {
                        result = await _signInManager.PasswordSignInAsync(signedUser.UserName!, loginModel.Password, loginModel.RememberMe, lockoutOnFailure: false);
                    }
                    else
                    {
                        result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, loginModel.RememberMe, lockoutOnFailure: false);
                    }
                    
                    if (result.Succeeded)
                    {
                        loginModel.LoginInValid = "";
                    }
                    else
                    {
                        // AddErrorsToModelState(result);
                        ModelState.AddModelError(string.Empty, "Invalid password");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid email address");
                }
            }
            return PartialView("_UserLoginPartial", loginModel);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegistrationModel registrationModel)
        {
            registrationModel.RegistrationInValid = "true";

            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = registrationModel.UserName,
                    Email = registrationModel.Email,
                    FirstName = registrationModel.FirstName,
                    LastName = registrationModel.LastName,
                    EmailConfirmed = true,
                    InstituteId = registrationModel.InstituteId
                };



                var result = await _userManager.CreateAsync(user, registrationModel.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");

                    Student student = new Student
                    {
                        StudentUserId = user.Id
                    };

                    _context.Add(student);
                    await _context.SaveChangesAsync();

                    registrationModel.RegistrationInValid = "";

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return PartialView("_UserRegistrationPartial", registrationModel);
                }

                AddErrorsToModelState(result);
            }

            List<Institute> institutes = await _instituteRepository.GetAvailableInstitutes();
            ViewBag.Institutes = new SelectList(institutes, "Id", "Name");

            return PartialView("_UserRegistrationPartial", registrationModel);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<bool> UserNameOrEmailExists(string userName, string email)
        {
            bool userNameExists = await _context.Users.AnyAsync(u => u.UserName!.ToUpper() == userName.ToUpper() || u.Email!.ToUpper() == email.ToUpper());

            if (userNameExists)
                return true;

            return false;

        }

        private void AddErrorsToModelState(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
