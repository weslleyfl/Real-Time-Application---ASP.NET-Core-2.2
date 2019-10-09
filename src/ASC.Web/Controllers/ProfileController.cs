using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ASC.Utilities;
using ASC.Web.Models.ViewModels;

namespace ASC.Web.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.FindByEmailAsync(HttpContext.User.GetCurrentUserDetails().Email);

            var profileModel = new ProfileViewModel
            {
                UserName = user.UserName,
                IsEditSuccess = false
            };

            return View(profileModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel profile)
        {
            if (!ModelState.IsValid)
            {
                return View(profile);
            }

            var user = await _userManager.FindByEmailAsync(HttpContext.User.GetCurrentUserDetails().Email);
            user.UserName = profile.UserName;

            var result = await _userManager.UpdateAsync(user);
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, false);

            profile.IsEditSuccess = result.Succeeded;

            AddErrors(result);

            if (ModelState.ErrorCount > 0)
                return View(profile);

            return RedirectToAction("Profile");

        }


        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        #endregion


    }
}