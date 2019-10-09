using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ASC.Web.Models;
using Microsoft.AspNetCore.Authorization;
using ASC.Models.BaseTypes;
using ASC.Utilities;
using ASC.Web.Models.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ASC.Web.Controllers
{
    public class CustomersController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public CustomersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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

    }
}