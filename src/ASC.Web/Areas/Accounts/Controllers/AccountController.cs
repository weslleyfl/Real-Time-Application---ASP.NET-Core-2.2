using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ASC.Web.Controllers;
using ASC.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Utilities;
using ASC.Web.Areas.Accounts.Models;
using ASC.Models.BaseTypes;

namespace ASC.Web.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        //IOptions<IdentityCookieOptions> identityCookieOptions,
        IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ServiceEngineers()
        {
            var serviceEngineers = await _userManager.GetUsersInRoleAsync(nameof(Roles.Engineer));
            IList<Claim> userClaims = null;


            foreach (var user in serviceEngineers)
            {
                userClaims = (await _signInManager.UserManager.GetClaimsAsync(user));
                user.IsActiveUser = Boolean.Parse(userClaims.FirstOrDefault(p => p.Type == "IsActive").Value);

            }

            // Hold all service engineers in session
            HttpContext.Session.SetSession("ServiceEngineers", serviceEngineers);

            return View(new ServiceEngineerViewModel
            {
                ServiceEngineers = serviceEngineers?.ToList(),
                Registration = new ServiceEngineerRegistrationViewModel() { IsEdit = false }
            });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel serviceEngineer)
        {
            serviceEngineer.ServiceEngineers = HttpContext.Session.GetSession<List<ApplicationUser>>("ServiceEngineers");

            if (!ModelState.IsValid)
            {
                return View(serviceEngineer);
            }

            if (serviceEngineer.Registration.IsEdit)
            {
                // Update User
                var user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
                user.UserName = serviceEngineer.Registration.UserName;
                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }

                // Update Password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult passwordResult = await _userManager.ResetPasswordAsync(user, token, serviceEngineer.Registration.Password);

                if (!passwordResult.Succeeded)
                {
                    passwordResult.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }

                // Update claims
                user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);

                IList<Claim> userClaims = await _signInManager.UserManager.GetClaimsAsync(user);

                //var isActiveClaim = user.Claims.SingleOrDefault(p => p.ClaimType == "IsActive");                
                var isActiveClaim = userClaims.FirstOrDefault(p => p.Type == "IsActive");

                var removeClaimResult = await _userManager.RemoveClaimAsync(user,
                    new System.Security.Claims.Claim(isActiveClaim.Type, isActiveClaim.Value));
                var addClaimResult = await _userManager.AddClaimAsync(user,
                    new System.Security.Claims.Claim(isActiveClaim.Type, serviceEngineer.Registration.IsActive.ToString()));
            }
            else
            {
                // Create User
                ApplicationUser user = new ApplicationUser
                {
                    UserName = serviceEngineer.Registration.UserName,
                    Email = serviceEngineer.Registration.Email,
                    EmailConfirmed = true
                };

                IdentityResult result = await _userManager.CreateAsync(user, serviceEngineer.Registration.Password);
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                                                 serviceEngineer.Registration.Email));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", serviceEngineer.Registration.IsActive.ToString()));

                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }

                // Assign user to Engineer Role
                var roleResult = await _userManager.AddToRoleAsync(user, nameof(Roles.Engineer));
                if (!roleResult.Succeeded)
                {
                    roleResult.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                    return View(serviceEngineer);
                }
            }

            // TODO: Não enviar email engineers
            if (serviceEngineer.Registration.IsActive)
            {
                await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email,
                    "Account Created/Modified",
                    $"Email : {serviceEngineer.Registration.Email} /n Passowrd : {serviceEngineer.Registration.Password}");
            }
            else
            {
                await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email,
                    "Account Deactivated",
                    $"Your account has been deactivated.");
            }

            return RedirectToAction("ServiceEngineers");
        }

        [HttpGet]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> Customers()
        {
            var users = await _userManager.GetUsersInRoleAsync(nameof(Roles.User));
            IList<Claim> userClaims = null;

            // Hold all service engineers in session
            HttpContext.Session.SetSession("Customers", users);

            foreach (var user in users)
            {
                userClaims = (await _signInManager.UserManager.GetClaimsAsync(user));
                user.IsActiveUser = Boolean.Parse(userClaims.FirstOrDefault(p => p.Type == "IsActive").Value);

            }

            return View(new CustomersViewModel
            {
                Customers = users?.ToList(),
                Registration = new CustomerRegistrationViewModel()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> Customers(CustomersViewModel customer)
        {
            if (!ModelState.IsValid) return View(customer);

            customer.Customers = HttpContext.Session.GetSession<List<ApplicationUser>>("Customers");

            var user = await _userManager.FindByEmailAsync(customer.Registration.Email);

            // Update claims 
            IList<Claim> userClaims = await _signInManager.UserManager.GetClaimsAsync(user);
            var isActiveClaim = userClaims.FirstOrDefault(p => p.Type == "IsActive");
            var removeClaimResult = await _userManager.RemoveClaimAsync(user, new Claim(isActiveClaim.Type, isActiveClaim.Value));
            var addClaimResult = await _userManager.AddClaimAsync(user, new Claim(isActiveClaim.Type, customer.Registration.IsActive.ToString()));

            //if (!customer.Registration.IsActive)
            //{
            //    await _emailSender.SendEmailAsync(customer.Registration.Email,
            //        "Account Deativated",
            //        $"Your account has been Deactivated!!!");
            //}

            return RedirectToAction("Customers");

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