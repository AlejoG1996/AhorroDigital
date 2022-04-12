using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Helpers;
using AhorroDigital.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AhorroDigital.API.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;

        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckTypeOfSavings();
            await CheckTyepeOfRetirement();
            await CheckDocumentTypesAsync();
            await CheckRolesAsycn();
            await CheckUserAsync("1017240121", "Alejandro", "Galeano", "admin@yopmail.com", "305 432 5671", "Calle 33 # 42 53", UserType.Admin, "2253564565");

        }
        private async Task CheckRolesAsycn()
        {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());

        }
        private async Task CheckUserAsync(string document, string firstName, string lastName, string email, string phoneNumber,
         string address, UserType userType, string acountnumber)
        {
            User user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                user = new User
                {
                    BankAccounts = acountnumber,
                    Address = address,
                    CountryCode = "57",
                    Document = document,
                    DocumentType = _context.documentTypes.FirstOrDefault(x => x.Description == "Cédula"),
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    UserName = email,
                    UserType = userType

                };

                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());


            }

        }
        private async Task CheckTypeOfSavings()
        {
            if (!_context.typeOfSavings.Any())
            {
                _context.typeOfSavings.Add(new TypeOfSaving { Description = "Ahorro Voluntario" });
                _context.typeOfSavings.Add(new TypeOfSaving { Description = "Ahorro Programado" });
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckTyepeOfRetirement()
        {
            if (!_context.typeOfRetirements.Any())
            {
                _context.typeOfRetirements.Add(new TypeOfRetirement { Description = "Retiro Total" });
                _context.typeOfRetirements.Add(new TypeOfRetirement { Description = "Retiro Parcial" });
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckDocumentTypesAsync()
        {
            if (!_context.documentTypes.Any())
            {
                _context.documentTypes.Add(new DocumentType { Description = "Cédula" });
                _context.documentTypes.Add(new DocumentType { Description = "Tarjeta de Identidad" });
                _context.documentTypes.Add(new DocumentType { Description = "NIT" });
                _context.documentTypes.Add(new DocumentType { Description = "Pasaporte" });
                await _context.SaveChangesAsync();
            }
        }
    }
}
