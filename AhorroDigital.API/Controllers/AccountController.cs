using AhorroDigital.API.Data;
using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Helpers;
using AhorroDigital.API.Models;
using AhorroDigital.Common.Enums;
using Core.Flash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Vereyon.Web;

namespace AhorroDigital.API.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IFlashMessage _flashMessage;

        public AccountController(IUserHelper userHelper, DataContext context, ICombosHelper combosHelper, IFlashMessage flasher)
        {
            _userHelper = userHelper;
            _flashMessage = flasher;
            _combosHelper = combosHelper;
            _context = context;
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
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


        public IActionResult NotAuthorized()
        {
            return View();
        }

        public IActionResult Register()
        {
            AddUserViewModel model = new AddUserViewModel
            {
                DocumentTypes = _combosHelper.GetComboDocumentTypes(),
                AccountTypes = _combosHelper.GetComboAccountTypes()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                //validar img
                string filename = "";
                if (model.ImageFile == null)
                {

                    filename = "noexiste.png";
                }
                else
                {
                    filename = model.ImageFile.FileName;
                }
                if (!Regex.IsMatch(filename.ToLower(), @"^.*\.(jpg|gif|png|jpeg)$"))
                {
                    _flashMessage.Danger("la imagen debe ser tipo .jpg .gift .png .jpeg");
                    model.DocumentTypes = _combosHelper.GetComboDocumentTypes();
                    model.AccountTypes = _combosHelper.GetComboAccountTypes();
                    return View(model);

                }

                // cedula no repetida
                User usertwo = await _context.Users
           .FirstOrDefaultAsync(x => x.Document == model.Document);
                if (usertwo != null)
                {

                    _flashMessage.Danger("El  Número de Documento ingresado ya está siendo usado.");
                    model.DocumentTypes = _combosHelper.GetComboDocumentTypes();
                    model.AccountTypes = _combosHelper.GetComboAccountTypes();
                    return View(model);


                }
                else
                {

                    User usertwos = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == model.UserName);

                    if (usertwos != null)
                    {
                        _flashMessage.Danger("El  Email ingresado ya está siendo usado.");
                        model.DocumentTypes = _combosHelper.GetComboDocumentTypes();
                        model.AccountTypes = _combosHelper.GetComboAccountTypes();
                        return View(model);
                    }
                }

                //almacenar foto
                string ruta = "";
                string path = "";
                string pic = "";
                if (model.ImageFile != null)
                {

                    pic = Path.GetFileName(model.Document.ToString() + ".png");

                    path = Path.Combine("wwwroot\\images\\users", pic);
                    ruta = "http://localhost:5047/images/users/" + pic;

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(stream);
                        model.ImageFullPath = ruta;

                    }


                }
                else
                {
                    model.ImageFullPath = "http://localhost:5047/images/users/noimages.png";
                }

                User user = await _userHelper.AddUserAsync(model, UserType.User);
                user.ImageFullPath = ruta;
                if(user==null)
                {
                    _flashMessage.Danger("Ya existe un usuario con la información ingresada, valide la información y intentelo nuevamente.");
                    model.DocumentTypes = _combosHelper.GetComboDocumentTypes();
                    model.AccountTypes = _combosHelper.GetComboAccountTypes();
                    return View(model);
                }

                LoginViewModel loginViewModel = new LoginViewModel
                {
                    Password = model.Password,
                    RememberMe = false,
                    Username=model.UserName
                };

                var resultado=await _userHelper.LoginAsync(loginViewModel);
                if (resultado.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

              
            }
            _flashMessage.Danger("Asegúrese de ingresar toda la información.");
            model.DocumentTypes = _combosHelper.GetComboDocumentTypes();
            model.AccountTypes = _combosHelper.GetComboAccountTypes();
            return View(model);
        }

    }
}
