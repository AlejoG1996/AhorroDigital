using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Helpers;
using AhorroDigital.Common.Enums;

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
            await CheckAccountTypesAsync();
            await CheckDocumentTypesAsync();
            await CheckRolesAsycn();
            await CheckUserAsync("1010", "Alejo", "Galeano", "luis@yopmail.com", "311 322 4620", "Calle Luna Calle Sol","0000000", "Bancolombia", UserType.Admin);
        }

        private async Task CheckUserAsync(string document, string firstName, string lastName, string email, string phoneNumber,
           string address,string accountnumber, string bank, UserType userType)
        {
            User user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                user = new User
                {
                    Address = address,
                    CountryCode = "57",
                    Document = document,
                    DocumentType = _context.DocumentTypes.FirstOrDefault(x => x.Name == "Cédula"),
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    AccountType = _context.AccountTypes.FirstOrDefault(x => x.Name == "Cuenta de Ahorro"),
                    UserName = email,
                    AccountNumber=accountnumber,
                    Bank = bank,
                    UserType = userType
                };

                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());

                
            }

        }
        private async Task CheckRolesAsycn()
        {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());
        }

        private async Task CheckAccountTypesAsync()
        {
            if(!_context.AccountTypes.Any())
            {
                _context.AccountTypes.Add(new AccountType { Name = "Cuenta de Ahorro" });
                _context.AccountTypes.Add(new AccountType { Name = "Cuenta Corriente" });
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckDocumentTypesAsync()
        {
            if (!_context.DocumentTypes.Any())
            {
                _context.DocumentTypes.Add(new DocumentType { Name = "Cédula" });
                
                await _context.SaveChangesAsync();
            }
        }
    }
}
