using AhorroDigital.API.Data;
using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Helpers;
using AhorroDigital.API.Models;
using AhorroDigital.Common.Enums;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using System.Data;
using System.Text.RegularExpressions;
using Vereyon.Web;

namespace AhorroDigital.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController:Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IConverterHelper _converterHelper;
        
        public UsersController(DataContext context, IFlashMessage flasher, IUserHelper userHelper, ICombosHelper combosHelper, IConverterHelper converterHelper
           )
        {
            _context = context;
            _flashMessage = flasher;
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            
        }

        public async Task<IActionResult> Index()
        {
            
            return View(await _context.Users
                .Include(x=>x.DocumentType)
                .Include(x=>x.Savings)
                .ThenInclude(x=>x.SavingType)
                .Include(x => x.AccountType)
                
                .ToListAsync());
        }

        //crea nuevo admin 

        public IActionResult Create()
        {
            UserViewModel model = new UserViewModel
            {
                DocumentTypes = _combosHelper.GetComboDocumentTypes(),
                AccountTypes = _combosHelper.GetComboAccountTypes()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
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

               
                //cedula no repetida
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
                    .FirstOrDefaultAsync(x => x.Email == model.Email);

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





                User user = await _converterHelper.ToUserAsync(model, true);
                user.UserType = UserType.Admin;
                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, user.UserType.ToString());


                _flashMessage.Info("Usuario Administrador creado con exito.");
                return RedirectToAction(nameof(Index));
            }

            model.DocumentTypes = _combosHelper.GetComboDocumentTypes();
            model.AccountTypes = _combosHelper.GetComboAccountTypes();
            return View(model);
        }

        //edita usuario
        public async Task<IActionResult>Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User user= await _userHelper.GetUserAsync(Guid.Parse(id));
            if(user == null)
            {
                return NotFound();

            }
           
            UserViewModel model = _converterHelper.ToUserViewModel(user);
          
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Edit(UserViewModel model)
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

                //almacenar foto
                string ruta = model.ImageFullPath;
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
                    model.ImageFullPath = ruta;
                }
                User user = await _converterHelper.ToUserAsync(model, false);
                await _userHelper.UpdateUserAsync(user);
                _flashMessage.Info("Usuario Editado  con exito.");

                return RedirectToAction(nameof(Index));
            }
            model.DocumentTypes=_combosHelper.GetComboDocumentTypes();
            model.AccountTypes = _combosHelper.GetComboAccountTypes();
            return View(model) ;
        }

        //elimina usuario
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }


            User user = await _userHelper.GetUserAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound();
            }

            string nombre = user.ImageFullPath.Split('/').Last();
            string path = Path.Combine("wwwroot\\images\\users", nombre);
            if (!nombre.Equals("noimages.png"))
            {
                System.IO.File.Delete(path);
            }
           

            await _userHelper.DeleteUserAsync(user);
            _flashMessage.Info("Usuario Eliminado  con exito.");
            return RedirectToAction(nameof(Index));
        }

        //detalles de usuario
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            User user = await _context.Users
                .Include(x => x.DocumentType)
                .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                 .ThenInclude(x=>x.Contributes)
                 .Include(x => x.Savings)
                 .ThenInclude(x => x.SavingType)
                 .FirstOrDefaultAsync(x=>x.Id== id);

            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }


        //agregar ahorro
        public async Task<IActionResult>AddSaving(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User user =await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .FirstOrDefaultAsync(x=>x.Id== id);

            if (user == null)
            {
                return NotFound();
            }

            SavingViewModel model = new SavingViewModel()
            {
                SavingTypes = _combosHelper.GetComboSavingTypes(),
                UserId = user.Id,

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>AddSaving(SavingViewModel model)
        {
            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .FirstOrDefaultAsync(x => x.Id == model.UserId);

            if(user== null) { return NotFound(); }

            Saving saving = await _converterHelper.ToSavingAsync(model, true);
            try
            {
                if (saving.SavingType.Name.Equals("Ahorro digital"))
                {
                    saving.MinValue = 0;
                }
                if (saving.SavingType.Name.Equals("Ahorro programado"))
                {
                    if(saving.MinValue<=0)
                    {
                        _flashMessage.Danger("El tipo de ahorro seleccionado debe tener un valor minimo a ahorrar diferente de 0.");
                        model.SavingTypes = _combosHelper.GetComboSavingTypes();
                        return View(model);
                    }

                }

                user.Savings.Add(saving);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Ahorro creado  con exito.");

                return RedirectToAction(nameof(Details), new {id=user.Id});
            }
            catch (Exception exception)
            {

                _flashMessage.Danger(string.Empty, exception.Message);

            }
            model.SavingTypes = _combosHelper.GetComboSavingTypes();
            return View(model);
        }

        //editar ahorro
        public async Task<IActionResult> EditSaving(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }

            Saving saving = await _context.Savings
                .Include(x => x.User)
                 .ThenInclude(x=>x.DocumentType)
                 .Include(x=>x.SavingType)
                  .Include(x => x.Contributes)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (saving == null)
            {
                return NotFound();
            }

            SavingViewModel model = _converterHelper.ToSavingViewModel(saving);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSaving(int id, SavingViewModel model)
        {

            if (id == null){
                return NotFound();
            }

            if (ModelState.IsValid)
            {
              

               

              
                try
                {
                    Saving saving = await _converterHelper.ToSavingAsync(model, false);
                    if (saving.SavingType.Name.Equals("Ahorro digital"))
                    {
                        if (saving.MinValue > 0)
                        {
                            _flashMessage.Danger("El tipo de ahorro seleccionado no debe tener valor minimo ahorrado ingrese el valor de 0 ");
                            model.SavingTypes = _combosHelper.GetComboSavingTypes();
                            return View(model);
                        }
                        saving.MinValue = 0;
                    }
                    if (saving.SavingType.Name.Equals("Ahorro programado"))
                    {
                        if (saving.MinValue <= 0)
                        {
                            _flashMessage.Danger("El tipo de ahorro seleccionado debe tener un valor minimo a ahorrar diferente de 0.");
                            model.SavingTypes = _combosHelper.GetComboSavingTypes();
                            return View(model);
                        }

                    }

                    _context.Savings.Update(saving);
                    await _context.SaveChangesAsync();
                    _flashMessage.Info("Ahorro Editado  con exito.");

                    return RedirectToAction(nameof(Details), new { id = model.UserId });
                }
                catch (Exception exception)
                {

                    _flashMessage.Danger(string.Empty, exception.Message);

                }
              
            }
            model.SavingTypes = _combosHelper.GetComboSavingTypes();
            return View(model);
        }



        public async Task<IActionResult> DeleteSaving(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Saving saving = await _context.Savings
                .Include(x => x.User)
                .Include(x => x.SavingType)
                .Include(x => x.Contributes)
              
                .FirstOrDefaultAsync(x => x.Id == id);
            if (saving == null)
            {
                return NotFound();
            }

            _context.Savings.Remove(saving);
            await _context.SaveChangesAsync();
            _flashMessage.Danger("Se elimino correctamente el ahorro ");

            return RedirectToAction(nameof(Details), new { id = saving.User.Id });
        }
    }
}
