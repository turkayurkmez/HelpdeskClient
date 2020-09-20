using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HelpdeskClient.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace HelpdeskClient.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        const string ADMIN_ROLE = "Admins";
        const string ADMIN_USERNAME = "Admin@email.com";
        private SignInManager<ApplicationUser> signInManager;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public RegisterModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;

        }
        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public async Task OnGetAsnc(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }
        
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var result = await userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                    var roleResult = await roleManager.FindByNameAsync(ADMIN_ROLE);
                    if (roleResult == null)
                    {
                        await roleManager.CreateAsync(new IdentityRole(ADMIN_ROLE));

                    }

                    if (user.UserName.ToLower() == ADMIN_USERNAME.ToLower())
                    {
                        await userManager.AddToRoleAsync(user, ADMIN_ROLE);
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }
    }

    public class InputModel
    {
        [Required, EmailAddress, Display(Name = "E-posta")]
        public string Email { get; set; }
        [Required, StringLength(100, ErrorMessage ="{0} en az {2} en fazla {1} karakter uzunluğunda olmalı", MinimumLength =6)]
        [DataType(DataType.Password), Display(Name ="Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password), Display(Name ="Şifre onay"), Compare("Password", ErrorMessage ="Şifre ve şifre onay aynı değil!")]
        public string ConfirmPassword { get; set; }
    }
}
