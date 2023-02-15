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

           
           

            await _userHelper.DeleteUserAsync(user);
            string nombre = user.ImageFullPath.Split('/').Last();
            string path = Path.Combine("wwwroot\\images\\users", nombre);
            if (!nombre.Equals("noimages.png"))
            {
                System.IO.File.Delete(path);
            }
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

        //detalle de ahorro
        public async Task<IActionResult> DetailsSaving(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }

            Saving saving = await _context.Savings
                .Include(x => x.User)
                .ThenInclude(x => x.AccountType)
                .Include(x => x.User)
                .ThenInclude(x => x.DocumentType)
                 .Include(x => x.SavingType)
                 .Include(x => x.Contributes)
                 .FirstOrDefaultAsync(x => x.Id == id);

            if (saving == null)
            {
                return NotFound();
            }
            return View(saving);
        }

        //nuevo contribuccion
        public async Task<IActionResult> AddContribute(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }

            Saving saving = await _context.Savings
                .Include(x => x.User)
                  .ThenInclude(x => x.DocumentType)
                  .Include(x => x.User)
                  .ThenInclude(x => x.AccountType)
              .Include(x => x.Contributes)
              .Include(x => x.SavingType)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (saving == null)
            {
                return NotFound();
            }

            ContributeViewModel model = new ContributeViewModel()
            {
              
                SavingId = saving.Id,

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContribute(ContributeViewModel model)
        {
            if(ModelState.IsValid)
            {
                Saving saving =await _context.Savings
                    .Include(x => x.User)
                    .ThenInclude(x=>x.DocumentType)
                     .Include(x => x.User)
                    .ThenInclude(x => x.AccountType)
                    .Include(x => x.SavingType)
                    .Include(x => x.Contributes)
                    .FirstOrDefaultAsync(x => x.Id == model.SavingId);

                if (saving == null) { return NotFound(); }
                if (model.Value <= 0)
                {
                    _flashMessage.Danger("Debes Ingresar Un valor a Ahorrar superior a 0");
                    return View(model);
                }
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
                    return View(model);

                }
                if (model.ImageFile == null)
                {
                    _flashMessage.Danger("debes ingresar foto del comprobante de la consignación del ahorro");
                    return View(model);
                }
                //validar estado
                //almacenar foto
                string ruta = "http://localhost:5047/images/comprobantes/noimage.png";
                string path = "";
                string pic = "";
                if (model.ImageFile != null)
                {

                    pic = Path.GetFileName(saving.User.Document.ToString() + saving.ContributesCount.ToString() + ".png");

                    path = Path.Combine("wwwroot\\images\\comprobantes", pic);
                    ruta = "http://localhost:5047/images/comprobantes/" + pic;

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(stream);


                    }


                }
                User admin = await _userHelper.GetUserAsync(User.Identity.Name);
                Contribute contribute = new Contribute
                {
                    Date = model.Date,
                    Marks = "",
                    MarksAdmin = model.MarksAdmin,
                    UserAdmin = admin,
                    State=model.State,
                    ImageFullPath=ruta
                };
                 if (model.State.Equals("Aprobado"))
                {
                    contribute.ValueAvail = model.Value;
                    contribute.Value = 0;
                    contribute.ValueSlop = 0;
                }
                else if(model.State.Equals("Denegado"))
                {
                    contribute.ValueSlop = model.Value;
                    contribute.ValueAvail = 0;
                    contribute.Value = 0;

                }
                else
                {
                    contribute.ValueSlop = 0;
                    contribute.ValueAvail = 0;
                    contribute.Value = model.Value;
                }
                if (saving.SavingType.Name.Equals("Ahorro programado"))
                {
                    if(model.Value <saving.MinValue )
                    {
                        _flashMessage.Danger("el valor ahorrado debe ser igual o superior a  "+saving.MinValue);
                        return View(model);
                    }
                }
                if (saving.Contributes == null)
                {
                    saving.Contributes = new List<Contribute>();
                }

                saving.Contributes.Add(contribute);
                _context.Savings.Update(saving);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Consignación registrada con exito.");
                return RedirectToAction(nameof(DetailsSaving), new { id = saving.Id });
            }
            return View(model);
        }

        //editar contribucion
        public async Task<IActionResult> EditContribute(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Contribute contribute=await _context.Contributes
                .Include(x=>x.Saving)
                .ThenInclude(x=>x.SavingType)
                .Include(x => x.Saving)
                .ThenInclude(x => x.User)
                 .ThenInclude(x => x.AccountType)
                  .Include(x => x.Saving)
                .ThenInclude(x => x.User)
                 .ThenInclude(x => x.DocumentType)
                  .FirstOrDefaultAsync(x => x.Id == id);
            if (contribute == null)
            {
                return NotFound();
            }

            ContributeViewModel model = new ContributeViewModel
            {
                MarksAdmin = contribute.MarksAdmin,
                Date = contribute.Date,
                State = contribute.State,
                ImageFullPath = contribute.ImageFullPath,
                SavingId=contribute.Saving.Id
            };
            if (model.State.Equals("Aprobado"))
            {
                model.Value = contribute.ValueAvail;
            }
            else if(model.State.Equals("Denegado")) {
                model.Value = contribute.ValueSlop;

            }
            else
            {
                model.Value = contribute.Value;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContribute(int id, ContributeViewModel model)
        {
            if(ModelState.IsValid)
            {
                if (model.Value <= 0)
                {
                    _flashMessage.Danger("Debes Ingresar Un valor a Ahorrar superior a 0");
                    return View(model);
                }

                Contribute contribute = await _context.Contributes.FindAsync(id);
                Saving saving = await _context.Savings
                .Include(x => x.User)
                .ThenInclude(x => x.DocumentType)
                .Include(x => x.User)
                .ThenInclude(x => x.AccountType)
                  .Include(x => x.Contributes)
                  .Include(x => x.SavingType)
                  .FirstOrDefaultAsync(x => x.Id == model.SavingId);
                if (saving.SavingType.Name.Equals("Ahorro programado"))
                {
                    if (model.Value < saving.MinValue)
                    {
                        _flashMessage.Danger("el valor ahorrado debe ser igual o superior a  " + saving.MinValue);
                        return View(model);
                    }
                }
                contribute.MarksAdmin = model.MarksAdmin;
                contribute.State=model.State;
                User admin = await _userHelper.GetUserAsync(User.Identity.Name);
                contribute.UserAdmin = admin;
                if (model.State.Equals("Aprobado"))
                {
                    contribute.ValueAvail = model.Value;
                    contribute.Value = 0;
                    contribute.ValueSlop = 0;
                }
                else if (model.State.Equals("Denegado"))
                {
                    contribute.ValueAvail = 0;
                    contribute.Value = 0;
                    contribute.ValueSlop = model.Value;

                }
                else
                {
                    contribute.ValueAvail = 0;
                    contribute.Value = model.Value;
                    contribute.ValueSlop = 0;
                }
                _context.Contributes.Update(contribute);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Consignación editada con exito.");
                return RedirectToAction(nameof(DetailsSaving), new { id = model.SavingId });
            }
            return View(model);
        }

        //eliminar contribucion
        public async Task<IActionResult> DeleteContribute(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contribute contribute = await _context.Contributes
                .Include(x=>x.Saving)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (contribute == null)
            {
                return NotFound();
            }

            _context.Contributes.Remove(contribute);
            await _context.SaveChangesAsync();
            string nombre = contribute.ImageFullPath.Split('/').Last();
            string path = Path.Combine("wwwroot\\images\\comprobantes", nombre);
            if (!nombre.Equals("noimages.png"))
            {
                System.IO.File.Delete(path);
            }
            _flashMessage.Danger("Se elimino correctamente la consignación ");

            return RedirectToAction(nameof(DetailsSaving), new { id = contribute.Saving.Id });
        }
    }
}
