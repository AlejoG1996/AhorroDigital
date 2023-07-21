using AhorroDigital.API.Data;
using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Helpers;
using AhorroDigital.API.Models;
using AhorroDigital.Common.Enums;
using AhorroDigital.Common.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using Vereyon.Web;
using static Mysqlx.Datatypes.Scalar.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AhorroDigital.API.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class UsersController : Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IMailHelper _mailHelper;


        public UsersController(DataContext context, IFlashMessage flasher, IUserHelper userHelper, ICombosHelper combosHelper, IConverterHelper converterHelper,
          IMailHelper mailHelper)
        {
            _context = context;
            _flashMessage = flasher;
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _mailHelper = mailHelper;

        }


        #region USUARIOS
        public async Task<IActionResult> Index()
        {

            return View(await _context.Users
                .Include(x => x.DocumentType)
                .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .Include(x => x.AccountType)
                  .Include(x => x.Loans)
                .ThenInclude(x => x.LoanType)
                .Include(x => x.Loans)
                .ThenInclude(x => x.Payments)
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

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                string tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendMail(model.Email,
                  "Ahorro Digital - Confirmación de cuenta",
                  $"<div style='width: 100%; height: 800px; background-color: #F2F4F4;' >" +
                        $"<div style='width: 100%; height: 130px; background-color: #27AE60; justify-content: center !important; align-items: center !important; text-align: center !important; color:#fff  !important;' >" +
                                $"<br>" +
                                $"<h1 style=' text-align: center !important; color: #fff !important; font-family: 'Roboto', sans-serif; padding-top: 20px; letter-spacing: 1px; font-size: 45px; '>" +
                                    user.FullName.ToUpper() +
                                $"</h1>" +
                                  $"<p style=' text-align: center !important; color: #fff !important; font-family: 'Roboto', sans-serif; padding-top: 20px; letter-spacing: 1px; font-size: 45px; '>" +
                                    "Felicidades falta poco para hacer parte de esta gran familia y obtener grandes beneficios" +
                                $"</p>" +

                                 $"<br>" +
                        $"</div>" +
                                $"<div style='width: 100%; justify-content: center; align-items: center; text-align: center;' >" +
                                      $"<br>" +
                                      $"<img src='cid:imagen' style='width: 350px; margin: auto;  '  >" +
                                      $"<br>" +
                                        $"<h1 style=' text-align: center !important; color: #2ECC71 !important; font-family: 'Roboto', sans-serif; padding-top: 20px; letter-spacing: 1px; font-size: 45px;  '>" +
                                      "WOOW!" +
                                    $"</h1>" +

                                               $"<p style=' text-align: center; color: #616A6B; font-family: 'Roboto', sans-serif; ' >" +
                                                    "Estas a un solo paso de pertenecer a la familia de AHORRO DIGITAL " +
                                               $"</p>" +
                                               $"<p style=' text-align: center; color: #616A6B; font-family: 'Roboto', sans-serif;  ' >" +
                                                    "confirma tu cuenta y accede a todos nuestros servicios y beneficios." +
                                               $"</p>" +
                                               $"<br>" +
                                               $"<a  href = \"{tokenLink}\" style=' height: 90px !important; width: 400px !important; padding: 15px !important; background-color: #27AE60 !important; color:#fff !important; border-radius: 15px !important; font-size: 20px !important;  cursor: pointer !important; text-decoration:none !important;' >" +
                                                    "Confirmar Cuenta!" +
                                               $"</a>" +
                                $"</div>" +

                  $"</div>");


                _flashMessage.Info("Usuario Administrador creado con exito, para el ingreso a la plataforma debe activar la cuenta con el link enviado al email registrado.");
                return RedirectToAction(nameof(Index));
            }

            model.DocumentTypes = _combosHelper.GetComboDocumentTypes();
            model.AccountTypes = _combosHelper.GetComboAccountTypes();
            return View(model);
        }

        //edita usuario
        public async Task<IActionResult> Edit(string id)
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

            UserViewModel model = _converterHelper.ToUserViewModel(user);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
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
            model.DocumentTypes = _combosHelper.GetComboDocumentTypes();
            model.AccountTypes = _combosHelper.GetComboAccountTypes();
            return View(model);
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

        #endregion

        #region  AHORROS
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
                 .ThenInclude(x => x.Contributes)
                 .Include(x => x.Savings)
                 .ThenInclude(x => x.SavingType)
                  .Include(x => x.Loans)
                 .ThenInclude(x => x.LoanType)
                  .Include(x => x.Loans)
                 .ThenInclude(x => x.Payments)
                 .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }


        //agregar ahorro
        public async Task<IActionResult> AddSaving(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .FirstOrDefaultAsync(x => x.Id == id);

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
        public async Task<IActionResult> AddSaving(SavingViewModel model)
        {
            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .FirstOrDefaultAsync(x => x.Id == model.UserId);

            if (user == null) { return NotFound(); }

            Saving saving = await _converterHelper.ToSavingAsync(model, true);
            try
            {

                DateTime fechaactual = DateTime.Now;
                string datePr = model.DateIni.ToString("dd-MM-yyyy");
                string dste = fechaactual.ToString("dd-MM-yyyy");
                if (datePr != dste)
                {
                    _flashMessage.Danger("Debes Seleccionar la fecha actual para poder realizar el registro del ahorro. ");
                    model.SavingTypes = _combosHelper.GetComboSavingTypes();
                    return View(model);
                }


                if (saving.SavingType.Name.Equals("Ahorro digital"))
                {
                    saving.MinValue = 0;
                }


                if (model.MinValue < saving.SavingType.MinValue)
                {
                    _flashMessage.Danger("El tipo de ahorro seleccionado debe tener un valor minimo a ahorrar de " + saving.SavingType.MinValue.ToString("C"));
                    model.SavingTypes = _combosHelper.GetComboSavingTypes();
                    return View(model);
                }

                if (saving.SavingType.MinValue == 0)
                {
                    if (model.MinValue > saving.SavingType.MinValue)
                    {
                        _flashMessage.Danger("El tipo de ahorro seleccionado debe tener un valor minimo de ahorro igual a $0.");
                        model.SavingTypes = _combosHelper.GetComboSavingTypes();
                        return View(model);
                    }
                }

                saving.DateEnd = saving.DateIni.AddDays(saving.SavingType.NumberDays);
                user.Savings.Add(saving);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Ahorro creado  con exito.");

                return RedirectToAction(nameof(Details), new { id = user.Id });
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
            if (id == null)
            {
                return NotFound();
            }

            Saving saving = await _context.Savings
                .Include(x => x.User)
                 .ThenInclude(x => x.DocumentType)
                 .Include(x => x.SavingType)
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

            if (id == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {





                try
                {
                    Saving saving = await _converterHelper.ToSavingAsync(model, false);
                    if (saving.SavingType.Name.Equals("Ahorro digital"))
                    {

                        saving.MinValue = 0;
                    }
                    if (model.MinValue < saving.SavingType.MinValue)
                    {
                        _flashMessage.Danger("El tipo de ahorro seleccionado debe tener un valor minimo a ahorrar de " + saving.SavingType.MinValue.ToString("C"));
                        model.SavingTypes = _combosHelper.GetComboSavingTypes();
                        return View(model);
                    }

                    if (saving.SavingType.MinValue == 0)
                    {
                        if (model.MinValue > saving.SavingType.MinValue)
                        {
                            _flashMessage.Danger("El tipo de ahorro seleccionado debe tener un valor minimo de ahorro igual a $0.");
                            model.SavingTypes = _combosHelper.GetComboSavingTypes();
                            return View(model);
                        }
                    }
                    saving.DateEnd = saving.DateIni.AddDays(saving.SavingType.NumberDays);
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
                .Include(x => x.Retreats)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (saving == null)
            {
                return NotFound();
            }

            _context.Savings.Remove(saving);
            await _context.SaveChangesAsync();
            _flashMessage.Info("Se elimino correctamente el ahorro ");

            return RedirectToAction(nameof(Details), new { id = saving.User.Id });
        }


        #endregion

        #region  APORTES
        //detalle de ahorro
        public async Task<IActionResult> DetailsSaving(int? id)
        {
            if (id == null)
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
                 .Include(x => x.User)
                .ThenInclude(x => x.Loans)
                 .ThenInclude(x => x.LoanType)
                  .Include(x => x.User)
                .ThenInclude(x => x.Loans)
                 .ThenInclude(x => x.Payments)
                   .Include(x => x.Retreats)
                 .FirstOrDefaultAsync(x => x.Id == id);

            User user = await _context.Users
               .Include(x => x.DocumentType)
               .Include(x => x.AccountType)
              .Include(x => x.Savings)
             .ThenInclude(x => x.SavingType)
           .Include(x => x.Savings)
             .ThenInclude(x => x.Contributes)
             .Include(x => x.Loans)
             .ThenInclude(x => x.LoanType)
               .Include(x => x.Loans)
             .ThenInclude(x => x.Payments)
              .FirstOrDefaultAsync(x => x.Id == saving.User.Id);

            saving.User = user;
            if (saving == null)
            {
                return NotFound();
            }
            return View(saving);
        }

        //nuevo contribuccion
        public async Task<IActionResult> AddContribute(int? id)
        {
            if (id == null)
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
            if (ModelState.IsValid)
            {
                Saving saving = await _context.Savings
                    .Include(x => x.User)
                    .ThenInclude(x => x.DocumentType)
                     .Include(x => x.User)
                    .ThenInclude(x => x.AccountType)
                    .Include(x => x.SavingType)
                    .Include(x => x.Contributes)
                    .FirstOrDefaultAsync(x => x.Id == model.SavingId);

                if (saving == null) { return NotFound(); }
                if (model.Value <= 0)
                {
                    _flashMessage.Danger("Debes Ingresar Un valor a Ahorrar superior a $0");
                    return View(model);
                }

                DateTime fechaactual = DateTime.Now;
                string datePr = model.Date.ToString("dd-MM-yyyy");
                string dste = fechaactual.ToString("dd-MM-yyyy");
                if (datePr != dste)
                {
                    _flashMessage.Danger("Debes Seleccionar la fecha actual para poder realizar el registro del aporte. ");

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

                    pic = Path.GetFileName(saving.User.Document.ToString() + saving.Id + saving.ContributesCount.ToString() + ".png");

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
                    Marks = model.Marks,
                    MarksAdmin = model.MarksAdmin,
                    UserAdmin = admin,
                    State = model.State,

                    ImageFullPath = ruta
                };
                if (model.State.Equals("Aprobado"))
                {
                    contribute.ValueAvail = model.Value;
                    contribute.Value = 0;
                    contribute.ValueSlop = 0;
                }
                else if (model.State.Equals("Denegado"))
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
                    if (model.Value < saving.MinValue)
                    {
                        _flashMessage.Danger("el valor ahorrado debe ser igual o superior a  " + saving.MinValue.ToString("C"));
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

            Contribute contribute = await _context.Contributes
                .Include(x => x.Saving)
                .ThenInclude(x => x.SavingType)
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
                SavingId = contribute.Saving.Id,
                Marks = contribute.Marks
            };
            if (model.State.Equals("Aprobado"))
            {
                model.Value = contribute.ValueAvail;
            }
            else if (model.State.Equals("Denegado"))
            {
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
            if (ModelState.IsValid)
            {
                if (model.Value <= 0)
                {
                    _flashMessage.Danger("Debes Ingresar Un valor a Ahorrar superior a $0");
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
                        _flashMessage.Danger("el valor ahorrado debe ser igual o superior a  " + saving.MinValue.ToString("C"));
                        return View(model);
                    }
                }

                //validar img
                string filename = "";
                if (model.ImageFile == null)
                {

                    filename = model.ImageFullPath;
                }
                else
                {
                    filename = model.ImageFile.FileName;
                }


                //almacenar foto
                string ruta = "http://localhost:5047/images/comprobantes/noimage.png";
                string path = "";
                string pic = "";
                if (model.ImageFile != null)
                {
                    if (!Regex.IsMatch(filename.ToLower(), @"^.*\.(jpg|gif|png|jpeg)$"))
                    {
                        _flashMessage.Danger("la imagen debe ser tipo .jpg .gift .png .jpeg");
                        return View(model);

                    }

                    pic = Path.GetFileName(contribute.ImageFullPath);

                    path = Path.Combine("wwwroot\\images\\comprobantes", pic);
                    ruta = "http://localhost:5047/images/comprobantes/" + pic;

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(stream);


                    }


                }

                contribute.Marks = model.Marks;
                contribute.MarksAdmin = model.MarksAdmin;
                contribute.State = model.State;
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
                .Include(x => x.Saving)
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
            _flashMessage.Info("Se elimino correctamente la consignación ");

            return RedirectToAction(nameof(DetailsSaving), new { id = contribute.Saving.Id });
        }

        #endregion

        #region PRESTAMOS
        //detalle de ahorro


        public async Task<IActionResult> DetailsLoan(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            User user = await _context.Users
                  .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .Include(x => x.Loans)
                .ThenInclude(x => x.LoanType)
                  .Include(x => x.Loans)
                .ThenInclude(x => x.Payments)
                 .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        //agregar prestamo
        public async Task<IActionResult> AddLoan(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                 .Include(x => x.Loans)
                .ThenInclude(x => x.LoanType)
                 .Include(x => x.Loans)
                .ThenInclude(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            LoanViewModel model = new LoanViewModel()
            {
                LoanTypes = _combosHelper.GetComboLoanTypes(),
                UserId = user.Id,


            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLoan(LoanViewModel model)
        {
            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .Include(x => x.Loans)
                .ThenInclude(x => x.LoanType)
                  .Include(x => x.Loans)
                .ThenInclude(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == model.UserId);

            if (user == null) { return NotFound(); }

            DateTime fechaactual = DateTime.Now;
            string datePr = model.DateS.ToString("dd-MM-yyyy");
            string dste = fechaactual.ToString("dd-MM-yyyy");
            if (datePr != dste)
            {
                _flashMessage.Danger("Debes Seleccionar la fecha actual para poder realizar el registro del aporte. ");
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);
            }

            if (model.Value <= 0)
            {
                _flashMessage.Danger("Debes Ingresar Un valor de Prestamo superior a $0");
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);
            }
            if (model.Value > user.AvailLoan)
            {
                _flashMessage.Danger("El valor del Préstamo no puede superar el valor disponible de : " + user.AvailLoan.ToString("C"));
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
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
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);

            }

            if (model.State.Equals("Aprobado") && model.ImageFile == null)
            {
                _flashMessage.Danger("El estado del préstamo es aprobado porfavor ingrese imagen del comprobante de la consigancion. ");
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);
            }

            LoanType lot = await _context.LoanTypes.FirstOrDefaultAsync(x => x.Id == model.LoanTypeId);
            if (model.Dues > lot.NumberDues) {
                _flashMessage.Danger("El número de cuotas no puede superar  :  " + lot.NumberDues + " Cuotas");
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);
            }
            lot = null;
            Loan loan = await _converterHelper.ToLoanAsync(model, true);
            try
            {


                if (loan.State.Equals("Aprobado"))
                {
                    loan.Value = model.Value;
                    loan.ValueD = 0;
                    loan.ValueP = 0;


                    //imagen si es aprado
                    //almacenar foto
                    string ruta = "http://localhost:5047/images/prestamos/noimages.png";
                    string path = "";
                    string pic = "";
                    if (model.ImageFile != null)
                    {

                        pic = Path.GetFileName(user.Document.ToString() + loan.Id.ToString() + user.Loans.Count().ToString() + ".png");

                        path = Path.Combine("wwwroot\\images\\prestamos", pic);
                        ruta = "http://localhost:5047/images/prestamos/" + pic;

                        using (FileStream stream = new FileStream(path, FileMode.Create))
                        {
                            model.ImageFile.CopyTo(stream);


                        }
                        loan.ImageFullPath = ruta;


                    }

                }
                else if (loan.State.Equals("Pendiente"))
                {
                    loan.Value = 0;
                    loan.ValueP = model.Value;
                    loan.ValueD = 0;

                    loan.ImageFullPath = "http://localhost:5047/images/prestamos/noimages.png";
                }
                else
                {
                    loan.Value = 0;
                    loan.ValueP = 0;
                    loan.ValueD = model.Value;

                    loan.ImageFullPath = "http://localhost:5047/images/prestamos/noimages.png";
                }
                user.Loans.Add(loan);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                DateTime dateini = loan.DateA.AddDays(30);

                int valuebase = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(loan.Value / loan.Dues)));
                int valuedeuda = loan.Value;
                if (loan.State.Equals("Aprobado"))
                {

                    for (int i = 0; i < loan.Dues; i++)
                    {
                        int valuninterst = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(valuedeuda * (loan.Interest / 100))));

                        PaymentPlan payments = new PaymentPlan()
                        {
                            Loan = await _context.Loans.FindAsync(loan.Id),
                            Date = dateini,
                            State = "Pendiente",
                            ValueCapital = valuebase,
                            ValueInt = valuninterst,
                            ValueTP = 0,
                            TotalCapital = valuebase,
                            TotalInterest = valuninterst,
                            DayArrearsM = 0,
                            ValueArrearsM = 0,
                            Pago = "No",
                        };
                        payments.PendientePago = payments.TotalValue;
                        _context.PaymentPlan.Add(payments);
                        _context.Loans.Update(loan);
                        await _context.SaveChangesAsync();
                        dateini = dateini.AddDays(30);
                        valuedeuda = valuedeuda - valuebase;
                    }

                }

                _flashMessage.Info("Préstamo creado  con exito.");

                return RedirectToAction(nameof(DetailsLoan), new { id = user.Id });
            }
            catch (Exception exception)
            {

                _flashMessage.Danger(string.Empty, exception.Message);

            }
            model.LoanTypes = _combosHelper.GetComboLoanTypes();
            return View(model);
        }

        //editar prestamo no aprobado
        public async Task<IActionResult> EditLoan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Loan loan = await _context.Loans
                .Include(x => x.User)
                 .ThenInclude(x => x.DocumentType)
                    .Include(x => x.User)
                 .ThenInclude(x => x.AccountType)
                 .Include(x => x.LoanType)
                  .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            LoanViewModel model = _converterHelper.ToLoanViewModel(loan);
            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .Include(x => x.Loans)
                .ThenInclude(x => x.LoanType)
                  .Include(x => x.Loans)
                .ThenInclude(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == model.UserId);
            if (model.State.Equals("Aprobado"))
            {
                model.Value = loan.Value;
            }
            else if (model.State.Equals("Pendiente"))
            {
                model.Value = loan.ValueP;
            }
            else
            {
                model.Value = loan.ValueD;
            }
            model.ValueAvail = user.AvailLoan;
            model.NumberLoan = user.Loans.Count();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoan(int id, LoanViewModel model)
        {
            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)

                .FirstOrDefaultAsync(x => x.Id == model.UserId);

            if (user == null) { return NotFound(); }
            if (model.Value <= 0)
            {
                _flashMessage.Danger("Debes Ingresar Un valor de Prestamo superior a $0");
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);
            }
            if (model.Value > model.ValueAvail)
            {
                double newvalue = Convert.ToDouble(model.ValueAvail);
                _flashMessage.Danger("El valor del Préstamo ni puede superar el valor disponible de : " + newvalue.ToString("C"));
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
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
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);

            }

            if (model.State.Equals("Aprobado") && model.ImageFile == null)
            {
                _flashMessage.Danger("El estado del préstamo es aprobado porfavor ingrese imagen del comprobante de la consigancion. ");
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);
            }
            LoanType lot = await _context.LoanTypes.FirstOrDefaultAsync(x => x.Id == model.LoanTypeId);
            if (model.Dues > lot.NumberDues)
            {
                _flashMessage.Danger("El número de cuotas no puede superar  :  " + lot.NumberDues + " Cuotas");
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);
            }
            lot = null;
            Loan loan = await _converterHelper.ToLoanAsync(model, false);
            try
            {


                if (loan.State.Equals("Aprobado"))
                {
                    loan.Value = model.Value;
                    loan.ValueD = 0;
                    loan.ValueP = 0;


                    //imagen si es aprado
                    //almacenar foto
                    string ruta = "http://localhost:5047/images/prestamos/noimages.png";
                    string path = "";
                    string pic = "";
                    if (model.ImageFile != null)
                    {

                        pic = Path.GetFileName(user.Document.ToString() + loan.Id.ToString() + model.NumberLoan.ToString() + ".png");

                        path = Path.Combine("wwwroot\\images\\prestamos", pic);
                        ruta = "http://localhost:5047/images/prestamos/" + pic;

                        using (FileStream stream = new FileStream(path, FileMode.Create))
                        {
                            model.ImageFile.CopyTo(stream);


                        }
                        loan.ImageFullPath = ruta;

                    }

                }
                else if (loan.State.Equals("Pendiente"))
                {
                    loan.Value = 0;
                    loan.ValueP = model.Value;
                    loan.ValueD = 0;

                    loan.ImageFullPath = "http://localhost:5047/images/prestamos/noimages.png";
                }
                else
                {
                    loan.Value = 0;
                    loan.ValueP = 0;
                    loan.ValueD = model.Value;

                    loan.ImageFullPath = "http://localhost:5047/images/prestamos/noimages.png";
                }

                loan.DateA = DateTime.Now;
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();
                DateTime dateini = loan.DateA.AddDays(30);

                int valuebase = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(loan.Value / loan.Dues)));
                int valuedeuda = loan.Value;
                if (loan.State.Equals("Aprobado"))
                {
                    for (int i = 0; i < loan.Dues; i++)
                    {
                        int valuninterst = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(valuedeuda * (loan.Interest / 100))));

                        PaymentPlan payments = new PaymentPlan()
                        {
                            Loan = await _context.Loans.FindAsync(loan.Id),
                            Date = dateini,
                            State = "Pendiente",
                            ValueCapital = valuebase,
                            ValueInt = valuninterst,
                            ValueTP = 0,
                            TotalCapital = valuebase,
                            TotalInterest = valuninterst,
                            Pago = "No",
                            DayArrearsM = 0,
                            ValueArrearsM = 0,
                        };
                        payments.PendientePago = payments.TotalValue;
                        _context.PaymentPlan.Add(payments);
                        _context.Loans.Update(loan);
                        await _context.SaveChangesAsync();
                        dateini = dateini.AddDays(30);
                        valuedeuda = valuedeuda - valuebase;
                    }

                }

                _flashMessage.Info("Préstamo editado  con exito.");

                return RedirectToAction(nameof(DetailsLoan), new { id = user.Id });
            }
            catch (Exception exception)
            {

                _flashMessage.Danger(string.Empty, exception.Message);

            }
            model.LoanTypes = _combosHelper.GetComboLoanTypes();
            return View(model);
        }

        //editar prestamo ya aprobado
        public async Task<IActionResult> EditLoanApr(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Loan loan = await _context.Loans
                .Include(x => x.User)
                 .ThenInclude(x => x.DocumentType)
                    .Include(x => x.User)
                 .ThenInclude(x => x.AccountType)
                 .Include(x => x.LoanType)
                  .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            LoanViewModel model = _converterHelper.ToLoanViewModel(loan);
            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
                .Include(x => x.Loans)
                .ThenInclude(x => x.LoanType)
                  .Include(x => x.Loans)
                .ThenInclude(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == model.UserId);
            if (model.State.Equals("Aprobado"))
            {
                model.Value = loan.Value;
            }
            else if (model.State.Equals("Pendiente"))
            {
                model.Value = loan.ValueP;
            }
            else
            {
                model.Value = loan.ValueD;
            }
            model.ValueAvail = user.AvailLoan;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoanApr(int id, LoanViewModel model)
        {
            User user = await _context.Users
                .Include(x => x.DocumentType)
                  .Include(x => x.AccountType)
                 .Include(x => x.Savings)
                .ThenInclude(x => x.SavingType)
              .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)

                .FirstOrDefaultAsync(x => x.Id == model.UserId);

            if (user == null) { return NotFound(); }



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
                model.LoanTypes = _combosHelper.GetComboLoanTypes();
                return View(model);

            }


            Loan loan = await _converterHelper.ToLoanAsync(model, false);
            try
            {




                //almacenar foto
                string ruta = "http://localhost:5047/images/prestamos/noimages.png";
                string path = "";
                string pic = "";
                if (model.ImageFile != null)
                {



                    pic = Path.GetFileName(model.ImageFullPath);
                    path = Path.Combine("wwwroot\\images\\prestamos", pic);
                    ruta = "http://localhost:5047/images/prestamos/" + pic;

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(stream);


                    }
                    loan.ImageFullPath = ruta;

                }




                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Préstamo editado  con exito.");

                return RedirectToAction(nameof(DetailsLoan), new { id = user.Id });
            }
            catch (Exception exception)
            {

                _flashMessage.Danger(string.Empty, exception.Message);

            }
            model.LoanTypes = _combosHelper.GetComboLoanTypes();
            return View(model);
        }

        //eliminar prestamo

        public async Task<IActionResult> DeleteLoan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Loan loan = await _context.Loans
                .Include(x => x.LoanType)
                  .Include(x => x.Payments)
                  .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (loan == null)
            {
                return NotFound();
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
            string nombre = loan.ImageFullPath.Split('/').Last();
            string path = Path.Combine("wwwroot\\images\\prestamos", nombre);
            if (!nombre.Equals("noimages.png"))
            {
                System.IO.File.Delete(path);
            }
            _flashMessage.Info("Se elimino correctamente el préstamo ");

            return RedirectToAction(nameof(DetailsLoan), new { id = loan.User.Id });

        }

        #endregion

        #region PAGOS
        //detalles pagos
        public async Task<IActionResult> DetailsPayment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Loan loan = await _context.Loans
                .Include(x => x.User)
                .ThenInclude(x => x.AccountType)
                .Include(x => x.User)
                .ThenInclude(x => x.DocumentType)
                 .Include(x => x.LoanType)
                 .Include(x => x.Payments)
                 .Include(x => x.User)
                .ThenInclude(x => x.Savings)
                 .ThenInclude(x => x.SavingType)
                  .Include(x => x.User)
                .ThenInclude(x => x.Savings)
                 .ThenInclude(x => x.Contributes)
                 .Include(x => x.PaymentF)
                 .FirstOrDefaultAsync(x => x.Id == id);



            User user = await _context.Users
                 .Include(x => x.DocumentType)
                 .Include(x => x.AccountType)
                .Include(x => x.Savings)
               .ThenInclude(x => x.SavingType)
             .Include(x => x.Savings)
               .ThenInclude(x => x.Contributes)
               .Include(x => x.Loans)
               .ThenInclude(x => x.LoanType)
                 .Include(x => x.Loans)
               .ThenInclude(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == loan.User.Id);

            loan.User = user;
            if (loan == null)
            {
                return NotFound();
            }
            return View(loan);
        }


        //realizar pagos nuevos parciales o totales
        public async Task<IActionResult> AddPayments(int id)
        {

            if (id == null)
            {
                return NotFound();
            }

            PaymentPlan paymentPlan = await _context.PaymentPlan
                .Include(x => x.Loan)
                  .FirstOrDefaultAsync(x => x.Id == id);

            if (paymentPlan == null)
            {
                return NotFound();
            }
            PaymentsPlantViewModel model = new PaymentsPlantViewModel()
            {

                Id = paymentPlan.Id,
                LoanId = paymentPlan.Loan.Id,
                Value = paymentPlan.TotalValue

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayments(int id, PaymentsPlantViewModel model)
        {

            Loan loan = await _context.Loans
                .Include(x => x.LoanType)
                  .Include(x => x.Payments)
                    .Include(x => x.PaymentF)
                      .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == model.LoanId);

            if (loan == null) { return NotFound(); }

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
                _flashMessage.Danger("Porfavor ingrese imagen del comprobante de la consigancion. ");
                return View(model);
            }


            DateTime fechaactual = DateTime.Now;
            string datePr = model.Date.ToString("dd-MM-yyyy");
            string dste = fechaactual.ToString("dd-MM-yyyy");
            if (datePr != dste)
            {
                _flashMessage.Danger("Debes Seleccionar la fecha actual para poder realizar el pago. ");
                return View(model);
            }

            try
            {
                //modifica plan 
                PaymentPlan paymentPlan = await _context.PaymentPlan.FirstOrDefaultAsync(x => x.Id == id);
                Payments payments = await _converterHelper.ToPaymentsPlanAsync(model, true);

                if (model.PaymentType.Equals("Cuota"))
                {


                    if (model.Value != paymentPlan.TotalValue)
                    {
                        _flashMessage.Danger("Debes Ingresar Un valor a pagar igual a  " + paymentPlan.TotalValue.ToString("C"));
                        return View(model);
                    }


                    if (model.State.Equals("Aprobado"))
                    {
                        paymentPlan.ValueTP = paymentPlan.ValueTP + Convert.ToInt16(model.Value);
                        paymentPlan.TotalCapital = 0;
                        paymentPlan.PendientePago = 0;
                        paymentPlan.TotalInterest = 0;
                        paymentPlan.DatePR = DateTime.Now;
                        paymentPlan.PaymentType = "Cuota";
                        paymentPlan.Pago = "Si";
                        paymentPlan.DayArrearsM = paymentPlan.DayArrears;
                        paymentPlan.ValueArrearsM = paymentPlan.ValueArrears;
                        paymentPlan.State = model.State;
                    }
                    else
                    {
                        paymentPlan.ValueTP = 0;
                        paymentPlan.TotalCapital = paymentPlan.ValueCapital;
                        paymentPlan.PendientePago = paymentPlan.ValueCapital + paymentPlan.ValueInt;
                        paymentPlan.TotalInterest = paymentPlan.ValueInt;
                        paymentPlan.DatePR = null;
                        paymentPlan.PaymentType = null;
                        paymentPlan.Pago = "Si";
                        paymentPlan.DayArrearsM = 0;
                        paymentPlan.ValueArrearsM = 0;
                        paymentPlan.State = model.State;
                    }



                    _context.PaymentPlan.Update(paymentPlan);
                    await _context.SaveChangesAsync();


                    payments.ValueCapital = paymentPlan.ValueCapital;
                    payments.ValueInt = paymentPlan.ValueInt;
                    payments.DayArrears = paymentPlan.DayArrears;
                    payments.ValueArrears = paymentPlan.ValueArrears;
                    payments.State = paymentPlan.State;
                    payments.IdPaymentPlan = paymentPlan.Id;
                }


                //pago parcial no disponible
                if (model.PaymentType.Equals("Parcial"))
                {
                    if (model.Value >= paymentPlan.PendientePago)
                    {
                        _flashMessage.Danger("Debes Ingresar Un valor a pagar inferior a  " + paymentPlan.PendientePago.ToString("C") + "   Si deseas realizar un pago mayor  selecciona la opción de pago a la cuota ");
                        return View(model);
                    }

                    int auxcapital = paymentPlan.TotalCapital;
                    int diferencia = 0;

                    if (model.State.Equals("Aprobado"))
                    {

                        paymentPlan.State = "Pendiente";

                        paymentPlan.ValueTP = paymentPlan.ValueTP + Convert.ToInt16(model.Value);
                        paymentPlan.TotalCapital = paymentPlan.TotalCapital - Convert.ToInt16(model.Value);
                        paymentPlan.PendientePago = paymentPlan.TotalValue - paymentPlan.ValueTP;
                        paymentPlan.DatePR = DateTime.Now;

                        if (paymentPlan.TotalCapital < 0)
                        {
                            paymentPlan.TotalCapital = 0;
                            diferencia = Convert.ToInt16(model.Value - auxcapital);
                            paymentPlan.TotalInterest = paymentPlan.TotalInterest - diferencia;

                            //para el pago
                            payments.ValueCapital = auxcapital;
                            payments.ValueInt = diferencia;

                        }
                        else
                        {
                            payments.ValueCapital = Convert.ToInt16(model.Value);
                            payments.ValueInt = 0;
                        }
                    }
                    else
                    {
                        paymentPlan.DatePR = null;
                    }



                    payments.State = model.State;
                    payments.IdPaymentPlan = paymentPlan.Id;
                    payments.Date = DateTime.Now;
                }

                // crear el pago
                User admin = await _userHelper.GetUserAsync(User.Identity.Name);

                payments.UserAdmin = admin;
                if (model.State.Equals("Aprobado"))
                {
                    payments.Value = Convert.ToInt16(model.Value);
                    payments.ValueP = 0;
                }
                else
                {
                    payments.Value = 0;
                    payments.ValueP = Convert.ToInt16(model.Value);
                }

                //almacenar foto
                string ruta = "http://localhost:5047/images/pagos/noimages.png";
                string path = "";
                string pic = "";
                if (model.ImageFile != null)
                {
                    string name = Convert.ToString(loan.PaymentF.Count() + 1);
                    pic = Path.GetFileName(loan.User.Document.ToString() + model.Id.ToString() + name + ".png");
                    path = Path.Combine("wwwroot\\images\\pagos", pic);
                    ruta = "http://localhost:5047/images/pagos/" + pic;

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(stream);


                    }
                    payments.ImageFullPath = ruta;
                }

                loan.PaymentF.Add(payments);
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Pago creado  con exito.");

                return RedirectToAction(nameof(DetailsPayment), new { id = loan.Id });
            }
            catch (Exception exception)
            {

                _flashMessage.Danger(string.Empty, exception.Message);

            }

            return View(model);


        }


        //realizar pagos  totales
        public async Task<IActionResult> AddNewPayments(int id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Loan loan = await _context.Loans
                .Include(x => x.LoanType)
                .Include(x => x.User)
                .Include(x => x.Payments)
                .Include(x => x.PaymentF)
                  .FirstOrDefaultAsync(x => x.Id == id);

            if (loan == null)
            {
                return NotFound();
            }
            NewPaymentViewModel model = new NewPaymentViewModel()
            {

                LoanId = loan.Id

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewPayments(int id, NewPaymentViewModel model)
        {

            Loan loan = await _context.Loans
                .Include(x => x.LoanType)
                  .Include(x => x.Payments)
                    .Include(x => x.PaymentF)
                      .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == model.LoanId);

            if (loan == null) { return NotFound(); }

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
                _flashMessage.Danger("Porfavor ingrese imagen del comprobante de la consigancion. ");
                return View(model);
            }

            DateTime fechaactual = DateTime.Now;
            string datePr = model.Date.ToString("dd-MM-yyyy");
            string dste = fechaactual.ToString("dd-MM-yyyy");
            if (datePr != dste)
            {
                _flashMessage.Danger("Debes Seleccionar la fecha actual para poder realizar el pago. ");
                return View(model);
            }

            try
            {



                // crear el pago
                User admin = await _userHelper.GetUserAsync(User.Identity.Name);
                Payments payments = await _converterHelper.ToPaymentsAsync(model, true);
                List<PaymentPlan> paymentsPlan = await _context.PaymentPlan.Where(x => x.Loan.Id == model.LoanId).ToListAsync();
                int valuecapital = 0, valueint = 0, valuearrears = 0, stateP = 0, vrft = 0;
                double dayarrear = 0;


                if (model.PaymentType.Equals("Total"))
                {

                    if (model.Value != loan.ValueTotal + loan.ValueArrearst)
                    {
                        int valor = loan.ValueArrearst + loan.ValueTotal;
                        _flashMessage.Danger("Si quieres hacer un pago total debes ingresar un valor a pagar igual a  " + valor.ToString("C"));
                        return View(model);
                    }

                    foreach (var item in paymentsPlan)
                    {


                        if (item.State.Equals("Pendiente") && item.ValueTP == 0)
                        {
                            valuecapital = valuecapital + item.TotalCapital;
                            valueint = valueint + item.ValueInt;
                            dayarrear = dayarrear + item.DayArrears;
                            valuearrears = valuearrears + item.ValueArrears;

                            stateP = stateP + 1;



                        }
                        else
                        {
                            vrft = vrft + item.PendientePago;
                        }


                    }



                    payments.ValueInt = Convert.ToInt16(valuecapital * (loan.Interest / 100));
                    payments.ValueCapital = valuecapital;
                    payments.DayArrears = dayarrear;
                    payments.ValueArrears = valuearrears;
                    payments.State = model.State;
                    payments.UserAdmin = admin;

                    if (model.State.Equals("Aprobado"))
                    {
                        payments.Value = model.Value;
                        payments.ValueP = 0;
                    }
                    else if (model.State.Equals("Pendiente"))
                    {
                        payments.Value = 0;
                        payments.ValueP = model.Value;
                    }

                    if (model.State.Equals("Aprobado"))
                    {

                        int vrtotral = loan.ValueTotal - vrft;
                        int auxvr = 0;
                        foreach (var item in paymentsPlan)
                        {
                            if (item.State.Equals("Pendiente"))
                            {
                                if (item.ValueTP == 0)
                                {
                                    if (item.ValueArrears > 0)
                                    {
                                        item.ValueTP = (valuecapital / stateP) + item.ValueArrears + (payments.ValueInt / stateP);

                                    }
                                    else
                                    {
                                        item.ValueTP = (valuecapital / stateP) + (payments.ValueInt / stateP);

                                    }
                                    item.ValueIntG = item.ValueInt;
                                    item.ValueArrearsM = item.ValueArrears;
                                    item.DayArrearsM = item.DayArrears;
                                    item.ValueCapital = (valuecapital / stateP);
                                    item.ValueInt = (payments.ValueInt / stateP);
                                    item.State = model.State;
                                    item.TotalCapital = 0;
                                    item.PendientePago = 0;
                                    item.TotalInterest = 0;
                                    item.DatePR = DateTime.Now;
                                    item.PaymentType = "Total";
                                }
                                else
                                {
                                    item.ValueTP = item.ValueCapital + item.ValueInt + item.ValueArrears;
                                    item.State = model.State;
                                    auxvr = item.PendientePago;
                                    item.TotalCapital = 0;
                                    item.PendientePago = 0;
                                    item.TotalInterest = 0;
                                    item.DatePR = DateTime.Now;
                                    item.PaymentType = "Total";
                                }
                                item.Pago = "SiTotal";
                                _context.PaymentPlan.Update(item);
                                await _context.SaveChangesAsync();

                            }



                            loan.ValueLoanPagado = loan.Value;
                            _context.Loans.Update(loan);
                            await _context.SaveChangesAsync();



                        }


                    }
                    else
                    {
                        int valued = payments.Loan.Value;
                        foreach (var item in paymentsPlan)
                        {
                            item.ValueArrearsM = 0;
                            item.DayArrearsM = 0;
                            item.ValueIntG = item.ValueInt;

                            valued = valued - item.ValueCapital;
                            item.PendientePago = item.ValueInt + item.ValueCapital;
                            item.TotalInterest = item.ValueInt;
                            item.TotalCapital = item.ValueCapital;
                            item.Pago = "SiTotal";
                            _context.PaymentPlan.Update(item);
                            await _context.SaveChangesAsync();
                        }


                    }





                }




                //almacenar foto
                string ruta = "http://localhost:5047/images/pagos/noimages.png";
                string path = "";
                string pic = "";
                if (model.ImageFile != null)
                {
                    string name = Convert.ToString(loan.PaymentF.Count() + 1);
                    pic = Path.GetFileName(loan.User.Document.ToString() + model.Id.ToString() + name + ".png");
                    path = Path.Combine("wwwroot\\images\\pagos", pic);
                    ruta = "http://localhost:5047/images/pagos/" + pic;

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(stream);


                    }
                    payments.ImageFullPath = ruta;
                }
                payments.Date = DateTime.Now;
                loan.PaymentF.Add(payments);
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();


                _flashMessage.Info("Pago creado  con exito.");

                return RedirectToAction(nameof(DetailsPayment), new { id = loan.Id });
            }
            catch (Exception exception)
            {

                _flashMessage.Danger(string.Empty, exception.Message);

            }

            return View(model);

        }

        //editar pagos parciales cuotas
        public async Task<IActionResult> EditPayment(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Payments payments = await _context.Payments
                .Include(x => x.Loan)
                .ThenInclude(x => x.LoanType)
                .Include(x => x.Loan)
                .ThenInclude(x => x.User)
                  .FirstOrDefaultAsync(x => x.Id == id);

            if (payments == null)
            {
                return NotFound();
            }


            PaymentsPlantViewModel model = new PaymentsPlantViewModel
            {
                MarksAdmin = payments.MarksAdmin,
                Marks = payments.Marks,
                Date = payments.Date,
                State = payments.State,
                PaymentType = payments.PaymentType,
                ImageFullPath = payments.ImageFullPath,
                LoanId = payments.Loan.Id,
                IdPaymentPlan = payments.IdPaymentPlan,
                Value = 0,


            };

            if (payments.State.Equals("Aprobado"))
            {
                model.Value = payments.Value;
            }
            else
            {
                model.Value = payments.ValueP;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPayment(int id, PaymentsPlantViewModel model)
        {


            Loan loan = await _context.Loans
                .Include(x => x.LoanType)

                      .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == model.LoanId);

            if (loan == null) { return NotFound(); }

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




            try
            {
                //modifica plan 

                Payments payments = await _converterHelper.ToPaymentsPlanAsync(model, false);
                PaymentPlan paymentPlan = await _context.PaymentPlan.FirstOrDefaultAsync(x => x.Id == model.IdPaymentPlan);
                string estado = "";
                if (model.PaymentType.Equals("Cuota"))
                {



                    estado = paymentPlan.State;

                    if (model.State.Equals("Aprobado"))
                    {
                        paymentPlan.ValueTP = paymentPlan.ValueTP + Convert.ToInt16(model.Value);
                        paymentPlan.TotalCapital = 0;
                        paymentPlan.PendientePago = 0;
                        paymentPlan.TotalInterest = 0;
                        paymentPlan.DatePR = DateTime.Now;
                        paymentPlan.PaymentType = "Cuota";
                        paymentPlan.Pago = "Si";
                        paymentPlan.DayArrearsM = paymentPlan.DayArrears;
                        paymentPlan.ValueArrearsM = paymentPlan.ValueArrears;
                        paymentPlan.State = model.State;
                    }
                    else
                    {
                        paymentPlan.ValueTP = 0;
                        paymentPlan.TotalCapital = paymentPlan.ValueCapital;
                        paymentPlan.PendientePago = paymentPlan.ValueCapital + paymentPlan.ValueInt;
                        paymentPlan.TotalInterest = paymentPlan.ValueInt;
                        paymentPlan.DatePR = null;
                        paymentPlan.PaymentType = null;
                        paymentPlan.Pago = "Si";
                        paymentPlan.DayArrearsM = 0;
                        paymentPlan.ValueArrearsM = 0;
                        paymentPlan.State = model.State;
                    }



                    _context.PaymentPlan.Update(paymentPlan);
                    await _context.SaveChangesAsync();


                    payments.ValueCapital = paymentPlan.ValueCapital;
                    payments.ValueInt = paymentPlan.ValueInt;
                    payments.DayArrears = paymentPlan.DayArrears;
                    payments.ValueArrears = paymentPlan.ValueArrears;
                    payments.State = paymentPlan.State;
                    payments.IdPaymentPlan = model.IdPaymentPlan;
                    payments.Date = DateTime.Now;
                }


                //no habilitado
                if (model.PaymentType.Equals("Parcial"))
                {


                    int auxcapital = paymentPlan.TotalCapital;
                    int diferencia = 0;

                    if (model.State.Equals("Aprobado"))
                    {

                        paymentPlan.State = "Pendiente";

                        paymentPlan.ValueTP = paymentPlan.ValueTP + Convert.ToInt16(model.Value);
                        paymentPlan.TotalCapital = paymentPlan.TotalCapital - Convert.ToInt16(model.Value);
                        paymentPlan.PendientePago = paymentPlan.TotalValue - paymentPlan.ValueTP;
                        paymentPlan.DatePR = DateTime.Now;

                        if (paymentPlan.TotalCapital < 0)
                        {
                            paymentPlan.TotalCapital = 0;
                            diferencia = Convert.ToInt16(model.Value - auxcapital);
                            paymentPlan.TotalInterest = paymentPlan.TotalInterest - diferencia;

                            //para el pago
                            payments.ValueCapital = auxcapital;
                            payments.ValueInt = diferencia;
                        }
                        else
                        {
                            payments.ValueCapital = Convert.ToInt16(model.Value);
                            payments.ValueInt = 0;
                        }
                    }
                    else
                    {
                        paymentPlan.State = "Pendiente";

                        paymentPlan.ValueTP = paymentPlan.ValueTP - Convert.ToInt16(model.Value);
                        paymentPlan.TotalCapital = paymentPlan.ValueCapital;
                        paymentPlan.PendientePago = paymentPlan.TotalValue - paymentPlan.ValueTP;
                        paymentPlan.TotalInterest = paymentPlan.ValueInt;
                        paymentPlan.DatePR = null;

                    }


                    payments.DayArrears = paymentPlan.DayArrears;
                    payments.ValueArrears = paymentPlan.ValueArrears;
                    payments.State = model.State;
                    payments.IdPaymentPlan = model.IdPaymentPlan;
                    payments.Date = DateTime.Now;
                }

                // crear el pago
                User admin = await _userHelper.GetUserAsync(User.Identity.Name);

                payments.UserAdmin = admin;
                if (model.State.Equals("Aprobado"))
                {
                    payments.Value = Convert.ToInt16(model.Value);
                    payments.ValueP = 0;

                }
                else
                {
                    payments.Value = 0;
                    payments.ValueP = Convert.ToInt16(model.Value);
                }

                if (model.ImageFile != null)
                {
                    //almacenar foto
                    string ruta = "http://localhost:5047/images/pagos/noimages.png";
                    string path = "";
                    string pic = "";
                    if (model.ImageFile != null)
                    {

                        pic = Path.GetFileName(model.ImageFullPath);
                        path = Path.Combine("wwwroot\\images\\pagos", pic);
                        ruta = "http://localhost:5047/images/pagos/" + pic;





                        using (FileStream stream = new FileStream(path, FileMode.Create))
                        {
                            model.ImageFile.CopyTo(stream);


                        }
                        payments.ImageFullPath = ruta;




                    }
                }


                _context.Payments.Update(payments);
                await _context.SaveChangesAsync();

                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Pago editado  con exito.");

                return RedirectToAction(nameof(DetailsPayment), new { id = loan.Id });
            }
            catch (Exception exception)
            {

                _flashMessage.Danger(string.Empty, exception.Message);

            }

            return View(model);


        }


        //editar pagos totales
        public async Task<IActionResult> EditNewPayment(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Payments payments = await _context.Payments
                .Include(x => x.Loan)
                .ThenInclude(x => x.LoanType)
                .Include(x => x.Loan)
                .ThenInclude(x => x.User)
                  .FirstOrDefaultAsync(x => x.Id == id);

            if (payments == null)
            {
                return NotFound();
            }

            Loan loan = await _context.Loans
            .Include(x => x.LoanType)
             .Include(x => x.Payments)
                  .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == payments.Loan.Id);


            NewPaymentViewModel model = new NewPaymentViewModel
            {
                MarksAdmin = payments.MarksAdmin,
                Marks = payments.Marks,
                Date = payments.Date,
                State = payments.State,
                PaymentType = payments.PaymentType,
                ImageFullPath = payments.ImageFullPath,
                LoanId = payments.Loan.Id,
                Value = 0,
                Deuda = loan.ValueTotal + loan.ValueArrearst

            };

            if (payments.State.Equals("Aprobado"))
            {
                model.Value = payments.Value;
            }
            else
            {
                model.Value = payments.ValueP;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNewPayment(int id, NewPaymentViewModel model)
        {

            Loan loan = await _context.Loans
              .Include(x => x.LoanType)

                    .Include(x => x.User)
              .FirstOrDefaultAsync(x => x.Id == model.LoanId);

            if (loan == null) { return NotFound(); }

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




            try
            {



                // crear el pago
                User admin = await _userHelper.GetUserAsync(User.Identity.Name);
                Payments payments = await _converterHelper.ToPaymentsAsync(model, false);
                List<PaymentPlan> paymentsPlan = await _context.PaymentPlan.Where(x => x.Loan.Id == model.LoanId).ToListAsync();
                int valuecapital = 0, valueint = 0, valuearrears = 0, stateP = 0, vrft = 0; ;
                double dayarrear = 0;


                if (model.PaymentType.Equals("Total"))
                {







                    if (model.State.Equals("Aprobado"))
                    {
                        payments.Value = model.Value;
                        payments.ValueP = 0;

                    }

                    else if (model.State.Equals("Pendiente"))
                    {
                        payments.Value = 0;
                        payments.ValueP = model.Value;
                    }

                    foreach (var item in paymentsPlan)
                    {


                        if (item.State.Equals("Pendiente") && item.ValueTP == 0)
                        {
                            valuecapital = valuecapital + item.TotalCapital;
                            valueint = valueint + item.ValueInt;
                            dayarrear = dayarrear + item.DayArrears;
                            valuearrears = valuearrears + item.ValueArrears;
                            stateP = stateP + 1;
                        }
                        else
                        {
                            vrft = vrft + item.PendientePago;
                        }
                    }


                    payments.ValueInt = Convert.ToInt16(valuecapital * (loan.Interest / 100));
                    payments.ValueCapital = valuecapital;
                    payments.DayArrears = dayarrear;
                    payments.ValueArrears = valuearrears;
                    payments.State = model.State;
                    payments.UserAdmin = admin;

                    if (model.State.Equals("Aprobado"))
                    {


                        int vrtotral = model.Value - vrft;
                        int auxvr = 0;

                        foreach (var item in paymentsPlan)
                        {

                            if (item.State.Equals("Pendiente"))
                            {


                                if (item.ValueTP == 0)
                                {
                                    item.ValueArrearsM = item.ValueArrears;
                                    item.DayArrearsM = item.DayArrears;
                                    item.ValueIntG = item.ValueInt;
                                    if (item.ValueArrears > 0)
                                    {
                                        item.ValueTP = (valuecapital / stateP) + item.ValueArrears + (payments.ValueInt / stateP);

                                    }
                                    else
                                    {
                                        item.ValueTP = (valuecapital / stateP) + (payments.ValueInt / stateP);

                                    }
                                    item.ValueCapital = (valuecapital / stateP);
                                    item.ValueInt = (payments.ValueInt / stateP);

                                    item.State = model.State;
                                    item.TotalCapital = 0;
                                    item.PendientePago = 0;
                                    item.TotalInterest = 0;
                                    item.DatePR = DateTime.Now;
                                    item.PaymentType = "Total";

                                }
                                else
                                {
                                    item.ValueTP = item.ValueCapital + item.ValueInt + item.ValueArrears;
                                    item.State = model.State;
                                    auxvr = item.PendientePago;
                                    item.TotalCapital = 0;
                                    item.PendientePago = 0;
                                    item.TotalInterest = 0;
                                    item.DatePR = DateTime.Now;
                                    item.PaymentType = "Total";
                                }
                                item.Pago = "SiTotal";
                                _context.PaymentPlan.Update(item);
                                await _context.SaveChangesAsync();
                            }



                        }



                        loan.ValueLoanPagado = loan.Value;
                        _context.Loans.Update(loan);
                        await _context.SaveChangesAsync();



                    }
                    else
                    {




                        int valued = payments.Loan.Value;
                        foreach (var item in paymentsPlan)
                        {
                            DateTime fecha = Convert.ToDateTime(item.DatePR);
                            string datePr = fecha.ToString("dd-MM-yyyy");
                            string date = model.Date.ToString("dd-MM-yyyy");

                            int auxid = 0, val = 0, ids = 0;

                            ids = model.LoanId;
                            DateTime dtaux = Convert.ToDateTime("01/01/2100");

                            if (item.State.Equals("Aprobado") && datePr == date && item.PaymentType.Equals("Total"))
                            {

                                item.ValueArrearsM = 0;
                                item.DayArrearsM = 0;
                                item.ValueCapital = (payments.Loan.Value / loan.Dues);
                                item.ValueInt = item.ValueIntG;
                                valued = valued - item.ValueCapital;





                                item.State = "Pendiente";
                                item.ValueTP = 0;
                                item.TotalCapital = item.ValueCapital;
                                item.TotalInterest = item.ValueInt;
                                item.PendientePago = item.ValueInt + item.ValueCapital;

                                item.DatePR = null;
                                item.PaymentType = null;
                                item.Pago = "SiTotal";

                                payments.ValueCapital = valuecapital;
                                payments.ValueInt = Convert.ToInt16(valuecapital * (loan.Interest / 100));

                                _context.PaymentPlan.Update(item);
                                await _context.SaveChangesAsync();

                            }



                        }
                        paymentsPlan = null;
                        if (model.State.Equals("Pendiente"))
                        {
                            payments.Value = 0;
                            payments.ValueP = model.Value;
                        }
                        loan.ValueLoanPagado = loan.Value;


                        //_context.Loans.Update(loan);
                        //await _context.SaveChangesAsync();


                    }





                }




                //almacenar foto
                if (model.ImageFile != null)
                {
                    //almacenar foto
                    string ruta = "http://localhost:5047/images/pagos/noimages.png";
                    string path = "";
                    string pic = "";
                    if (model.ImageFile != null)
                    {

                        pic = Path.GetFileName(model.ImageFullPath);
                        path = Path.Combine("wwwroot\\images\\pagos", pic);
                        ruta = "http://localhost:5047/images/pagos/" + pic;





                        using (FileStream stream = new FileStream(path, FileMode.Create))
                        {
                            model.ImageFile.CopyTo(stream);


                        }
                        payments.ImageFullPath = ruta;




                    }
                }

                payments.Date = DateTime.Now;
                _context.Payments.Update(payments);
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();


                _flashMessage.Info("Pago editado  con exito.");

                return RedirectToAction(nameof(DetailsPayment), new { id = loan.Id });
            }
            catch (Exception exception)
            {

                _flashMessage.Danger(string.Empty, exception.Message);

            }

            return View(model);
        }


        //eliminar pago

        public async Task<IActionResult> DeletePayment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Payments payments = await _context.Payments
                .Include(x => x.Loan)
                  .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);


            if (payments == null)
            {
                return NotFound();
            }

            if (payments.PaymentType.Equals("Cuota"))
            {
                PaymentPlan paymentPlan = await _context.PaymentPlan.Where(x => x.Id == payments.IdPaymentPlan).FirstOrDefaultAsync();

                paymentPlan.ValueTP = 0;
                paymentPlan.TotalCapital = paymentPlan.ValueCapital;
                paymentPlan.PendientePago = paymentPlan.ValueCapital + paymentPlan.ValueInt;


                paymentPlan.TotalInterest = paymentPlan.ValueInt;
                paymentPlan.DatePR = null;
                paymentPlan.State = "Pendiente";
                paymentPlan.Pago = "No";
                _context.PaymentPlan.Update(paymentPlan);
                await _context.SaveChangesAsync();

            }
            else if (payments.PaymentType.Equals("Parcial"))
            {
                PaymentPlan paymentPlan = await _context.PaymentPlan.Where(x => x.Id == payments.IdPaymentPlan).FirstOrDefaultAsync();

                paymentPlan.ValueTP = 0;
                paymentPlan.TotalCapital = paymentPlan.ValueCapital;

                paymentPlan.PendientePago = paymentPlan.TotalValue - paymentPlan.ValueTP;

                paymentPlan.TotalInterest = paymentPlan.ValueInt;
                paymentPlan.DatePR = null;
                paymentPlan.State = "Pendiente";

                _context.PaymentPlan.Update(paymentPlan);
                await _context.SaveChangesAsync();
            }
            else if (payments.PaymentType.Equals("Total"))
            {
                List<PaymentPlan> paymentsPlan = await _context.PaymentPlan.Where(x => x.Loan.Id == payments.Loan.Id).ToListAsync();
                int valued = payments.Loan.Value;
                int auxvr = 0;
                foreach (var item in paymentsPlan)
                {
                    DateTime fecha = Convert.ToDateTime(item.DatePR);
                    string datePr = fecha.ToString("dd-MM-yyyy");
                    string date = payments.Date.ToString("dd-MM-yyyy");

                    int auxid = 0, val = 0, ids = 0;

                    DateTime dtaux = Convert.ToDateTime("01/01/2100");
                    if (item.State.Equals("Aprobado") && datePr == date && item.PaymentType.Equals("Total"))
                    {

                        item.State = "Pendiente";
                        item.ValueTP = 0;
                        item.TotalCapital = item.ValueCapital;
                        item.TotalInterest = item.ValueInt;
                        item.PendientePago = item.ValueInt + item.ValueCapital;
                        item.DatePR = null;
                        item.PaymentType = null;
                        item.Pago = "No";
                        item.ValueCapital = (payments.Loan.Value / payments.Loan.Dues);
                        item.ValueInt = item.ValueIntG;


                        _context.PaymentPlan.Update(item);
                        await _context.SaveChangesAsync();

                    }
                    if (!item.Pago.Equals("No"))
                    {
                        item.Pago = "No";
                    }
                    valued = valued - item.ValueCapital;
                }
            }

            _context.Payments.Remove(payments);
            await _context.SaveChangesAsync();
            string nombre = payments.ImageFullPath.Split('/').Last();
            string path = Path.Combine("wwwroot\\images\\pagos", nombre);
            if (!nombre.Equals("noimages.png"))
            {
                System.IO.File.Delete(path);
            }
            _flashMessage.Info("Se elimino correctamente el pago ");

            return RedirectToAction(nameof(DetailsPayment), new { id = payments.Loan.Id });
        }


        public JsonResult GetInterest(int LoanTypeId)
        {
            LoanType loanType = _context.LoanTypes
                .FirstOrDefault(c => c.Id == LoanTypeId);
            if (loanType == null)
            {
                return null;
            }


            List<string> info = new List<string>();

            info.Add(loanType.Interes.ToString());
            info.Add(loanType.NumberDues.ToString());
            info.Add(loanType.Name.ToString());
            info.Add(loanType.Marks.ToString());
            return Json(info);
        }

        public JsonResult GetDues(int loanTypeid)
        {
            LoanType loanType = _context.LoanTypes
                .FirstOrDefault(c => c.Id == loanTypeid);
            if (loanType == null)
            {
                return null;
            }


            List<string> info = new List<string>();

            info.Add(loanType.Interes.ToString());
            info.Add(loanType.NumberDues.ToString());
            info.Add(loanType.Name.ToString());
            info.Add(loanType.Marks.ToString());
            return Json(info);
        }

        public JsonResult GetSaving(int SavingTypeId)
        {
            SavingType saving = _context.SavingTypes
                .FirstOrDefault(c => c.Id == SavingTypeId);
            if (saving == null)
            {
                return null;
            }


            List<string> info = new List<string>();


            info.Add(saving.Name.ToString());
            info.Add(saving.Marks.ToString());
            return Json(info);
        }

        #endregion

        #region  RETIROS

        public async Task<IActionResult> DetailsRetreat(int? id)
        {
            if (id == null)
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
                 .Include(x => x.User)
                .ThenInclude(x => x.Loans)
                 .ThenInclude(x => x.LoanType)
                  .Include(x => x.User)
                .ThenInclude(x => x.Loans)
                 .ThenInclude(x => x.Payments)
                   .Include(x => x.Retreats)
                 .FirstOrDefaultAsync(x => x.Id == id);

            User user = await _context.Users
               .Include(x => x.DocumentType)
               .Include(x => x.AccountType)
              .Include(x => x.Savings)
             .ThenInclude(x => x.SavingType)
           .Include(x => x.Savings)
             .ThenInclude(x => x.Contributes)
             .Include(x => x.Loans)
             .ThenInclude(x => x.LoanType)
               .Include(x => x.Loans)
             .ThenInclude(x => x.Payments)
           
              .FirstOrDefaultAsync(x => x.Id == saving.User.Id);

            saving.User = user;
            if (saving == null)
            {
                return NotFound();
            }
            return View(saving);
        }

        //agregar prestamo
        public async Task<IActionResult> AddRetreat(int? id)
        {
            if (id == null)
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
               .Include(x => x.Retreats)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (saving == null)
            {
                return NotFound();
            }

            RetreatViewModel model = new RetreatViewModel()
            {

                SavingId = saving.Id,

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRetreat(RetreatViewModel model)
        {

            if (ModelState.IsValid)
            {
                Saving saving = await _context.Savings
                      .Include(x => x.User)
                      .ThenInclude(x => x.DocumentType)
                       .Include(x => x.User)
                      .ThenInclude(x => x.AccountType)
                      .Include(x => x.SavingType)
                      .Include(x => x.Contributes)
                       .Include(x => x.Retreats)
                        .Include(x => x.User)
                           .ThenInclude(x => x.Loans)
                           .ThenInclude(x => x.PaymentF)
                          .Include(x => x.User)
                           .ThenInclude(x => x.Loans)
                           .ThenInclude(x => x.Payments)
                      .FirstOrDefaultAsync(x => x.Id == model.SavingId);

                if (saving == null) { return NotFound(); }

                DateTime fechaactual = DateTime.Now;
                string datePr = model.DateS.ToString("dd-MM-yyyy");
                string dste = fechaactual.ToString("dd-MM-yyyy");
                if (datePr != dste)
                {
                    _flashMessage.Danger("Debes Seleccionar la fecha actual para poder realizar el retiro. ");
                    return View(model);
                }

                if (model.DateS< saving.DateEnd)
                {
                    _flashMessage.Danger("Debes esperar hasta la fecha: "+ saving.DateEnd.ToString("dd-MM-yyyy") + " para poder hacer retiro del valor ahorrado.");
                    return View(model);
                }


                if (model.Value <= 0)
                {
                    _flashMessage.Danger("Debes Ingresar Un valor de retiro superior a $0");

                    return View(model);
                }

                int vrd = saving.User.TotalA * 2;

                if (saving.User.AvailLoan != vrd)
                {
                    _flashMessage.Danger("No esposible registrar el retiro dado a que tienes algun préstamo pendiente por pagar.");

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

                Retreat retreat = await _converterHelper.ToRetreatAsync(model, true);
                if (model.Value != saving.User.TotalA)
                {
                    _flashMessage.Danger("El valor a retirar debe ser igual al valor ahorrado: " + saving.User.TotalA.ToString("C"));
                    return View(model);
                }
                //almacenar foto
                string ruta = "http://localhost:5047/images/retiros/noimages.png";
               


                User admin = await _userHelper.GetUserAsync(User.Identity.Name);
                retreat.UserAdmin = admin.FullName;

                if (retreat.State.Equals("Aprobado"))
                {
                    string path = "";
                    string pic = "";
                    if (model.ImageFile != null)
                    {

                        pic = Path.GetFileName(saving.User.Document.ToString() + model.SavingId.ToString() + ".png");
                        path = Path.Combine("wwwroot\\images\\retiros", pic);
                        ruta = "http://localhost:5047/images/retiros/" + pic;

                        using (FileStream stream = new FileStream(path, FileMode.Create))
                        {
                            model.ImageFile.CopyTo(stream);


                        }
                        retreat.ImageFullPath = ruta;
                    }


                    List<Contribute> abonos = await _context.Contributes.Where(x => x.Saving.Id == saving.Id).ToListAsync();
                        foreach (var items in abonos)
                        {
                            if (items.State.Equals("Aprobado"))
                            {
                                items.State = "Retirado";
                                items.ValueRetreat = items.ValueAvail;
                                items.ValueAvail = 0;

                                _context.Contributes.Update(items);
                                await _context.SaveChangesAsync();
                            }


                        

                    }



                }
                else
                {
                    retreat.ImageFullPath = ruta;
                }


                saving.Retreats.Add(retreat);
                _context.Savings.Update(saving);
                await _context.SaveChangesAsync();



                _flashMessage.Info("Retiro creado  con exito.");

                return RedirectToAction(nameof(DetailsRetreat), new { id = saving.Id });

            }

            return View(model);
        }


        public async Task<IActionResult> EditRetreat(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Retreat retreat = await _context.Retreats
                .Include(x => x.Saving)
                .ThenInclude(x => x.SavingType)
                .Include(x => x.Saving)
                .ThenInclude(x => x.User)
                 .ThenInclude(x => x.AccountType)
                  .Include(x => x.Saving)
                .ThenInclude(x => x.User)
                 .ThenInclude(x => x.DocumentType)
                  .Include(x => x.Saving)
                .ThenInclude(x => x.User)
                 .ThenInclude(x => x.Loans)
                 .ThenInclude(x => x.PaymentF)
                  .Include(x => x.Saving)
                .ThenInclude(x => x.User)
                 .ThenInclude(x => x.Loans).ThenInclude(x => x.Payments)
                  .FirstOrDefaultAsync(x => x.Id == id);
            if (retreat == null)
            {
                return NotFound();
            }

            RetreatViewModel model = new RetreatViewModel
            {
                MarksAdmin = retreat.MarksAdmin,
                Marks=retreat.Marks,
                DateS = retreat.DateS,
                State = retreat.State,
                ImageFullPath = retreat.ImageFullPath,
                SavingId = retreat.Saving.Id,
                Value= retreat.Value
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRetreat(int id, RetreatViewModel model)
        {
            if (ModelState.IsValid)
            {
                Saving saving = await _context.Savings
                      .Include(x => x.User)
                      .ThenInclude(x => x.DocumentType)
                       .Include(x => x.User)
                      .ThenInclude(x => x.AccountType)
                      .Include(x => x.SavingType)
                      .Include(x => x.Contributes)
                        .Include(x => x.User)
                           .ThenInclude(x => x.Loans)
                           .ThenInclude(x => x.PaymentF)
                          .Include(x => x.User)
                           .ThenInclude(x => x.Loans)
                           .ThenInclude(x => x.Payments)
                      .FirstOrDefaultAsync(x => x.Id == model.SavingId);

                if (saving == null) { return NotFound(); }

                DateTime fechaactual = DateTime.Now;
               
                string dste = fechaactual.ToString("dd-MM-yyyy");
               
                //if (model.dates < saving.dateend)
                //{
                //    _flashmessage.danger("debes esperar hasta la fecha: " + saving.dateend.tostring("dd-mm-yyyy") + " para poder hacer retiro del valor ahorrado.");
                //    return view(model);
                //}


                if (model.Value <= 0)
                {
                    _flashMessage.Danger("Debes Ingresar Un valor de retiro superior a $0");

                    return View(model);
                }

                int vrd = saving.User.TotalA * 2;

                if (saving.User.AvailLoan != vrd)
                {
                    _flashMessage.Danger("No esposible registrar el retiro dado a que tienes algun préstamo pendiente por pagar.");

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

                Retreat retreat = await _converterHelper.ToRetreatAsync(model, false);
               
                //almacenar foto
                string ruta = "http://localhost:5047/images/retiros/noimages.png";
                string path = "";
                string pic = "";
                if (model.ImageFile != null)
                {

                    pic = Path.GetFileName(saving.User.Document.ToString() + model.SavingId.ToString() + ".png");
                    path = Path.Combine("wwwroot\\images\\retiros", pic);
                    ruta = "http://localhost:5047/images/retiros/" + pic;

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(stream);


                    }
                    retreat.ImageFullPath = ruta;
                }


                User admin = await _userHelper.GetUserAsync(User.Identity.Name);
                retreat.UserAdmin = admin.FullName;

                if (retreat.State.Equals("Aprobado"))
                {



                    List<Contribute> abonos = await _context.Contributes.Where(x => x.Saving.Id == saving.Id).ToListAsync();
                    foreach (var items in abonos)
                    {
                        if (items.State.Equals("Aprobado"))
                        {
                            items.State = "Retirado";
                            items.ValueRetreat = items.ValueAvail;
                            items.ValueAvail = 0;

                            _context.Contributes.Update(items);
                            await _context.SaveChangesAsync();
                        }




                    }



                } else
                {
                    List<Contribute> abonos = await _context.Contributes.Where(x => x.Saving.Id == saving.Id).ToListAsync();
                    foreach (var items in abonos)
                    {
                        if (items.State.Equals("Retirado"))
                        {
                            items.State = "Aprobado";
                            items.ValueAvail = items.ValueRetreat;
                            items.ValueRetreat = 0;
                            

                            _context.Contributes.Update(items);
                            await _context.SaveChangesAsync();
                        }




                    }
                }


                _context.Retreats.Update(retreat);
                _context.Savings.Update(saving);
                await _context.SaveChangesAsync();



                _flashMessage.Info("Retiro Editado  con exito.");

                return RedirectToAction(nameof(DetailsRetreat), new { id = saving.Id });

            }

            return View(model);
        }

        public async Task<IActionResult> DeleteRetreat(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Retreat retrat = await _context.Retreats
                .Include(x => x.Saving)
                .ThenInclude(x => x.Contributes)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (retrat == null)
            {
                return NotFound();
            }
            List<Contribute> abonos = await _context.Contributes.Where(x => x.Saving.Id == retrat.Saving.Id).ToListAsync();
            foreach (var items in abonos)
            {
                if (items.State.Equals("Retirado"))
                {
                    items.State = "Aprobado";
                    items.ValueAvail = items.ValueRetreat;
                    items.ValueRetreat = 0;


                    _context.Contributes.Update(items);
                    await _context.SaveChangesAsync();
                }




            }

            _context.Retreats.Remove(retrat);
            await _context.SaveChangesAsync();
            string nombre = retrat.ImageFullPath.Split('/').Last();
            string path = Path.Combine("wwwroot\\images\\retiros", nombre);
            if (!nombre.Equals("noimages.png"))
            {
                System.IO.File.Delete(path);
            }
            _flashMessage.Info("Se elimino correctamente el retiro ");

            return RedirectToAction(nameof(DetailsRetreat), new { id = retrat.Saving.Id });
        }

        public JsonResult GetImagenes(string Email)
        {
            User user = _context.Users
                 .FirstOrDefault(c => c.Email == Email);
            List<string> info = new List<string>();

            if (user == null || Email == null)
            {
                info.Add("http://localhost:5047/images/users/noimages.png");

            }
            else
            {
                info.Add(user.ImageFullPath.ToString());

            }





            return Json(info);
        }

        #endregion

        #region Ahorros No Admin

        public async Task<IActionResult> MySavings()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
           
            if (user == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        public async Task<IActionResult> MyLoans()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(DetailsLoan), new { id = user.Id });
        }

        #endregion

    }


}
