using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userEmail, string userPassword)
        {
            var userInfo = await userManager.FindByEmailAsync(email: userEmail);
            if (userInfo == null)
            {
                return View();
            }

            //locoutOnFailure : 3 kez hatalı girince locklansın mı?
            //isPersistent : Browser kapatıldığında kalsın mı sigin işlemi yoksa tekrar mı login olsun. (cookie süresi 60 yada 120 gün olabilir)
            var signInResult = await signInManager.PasswordSignInAsync(userInfo, password: userPassword, isPersistent: true, lockoutOnFailure: false);
            if (signInResult == null || !signInResult.Succeeded)
            {
                return View();
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}