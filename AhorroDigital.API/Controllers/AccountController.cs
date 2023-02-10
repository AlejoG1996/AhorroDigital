using AhorroDigital.API.Helpers;
using AhorroDigital.API.Models;
using Core.Flash;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace AhorroDigital.API.Controllers
{
    public class AccountController:Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IFlashMessage _flashMessage;

        public AccountController(IUserHelper userHelper, IFlashMessage flasher)
        {
            _userHelper= userHelper;
            _flashMessage = flasher;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index), "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(Request.Query["ReturnUrl"].First());
                    }

                    return RedirectToAction("Index", "Home");
                }
                _flashMessage.Danger(string.Empty, "Email o contraseña incorrectos.");
               
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("IndexHome", "Home");
        }
    }
}
