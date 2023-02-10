using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AhorroDigital.API.Models;

namespace AhorroDigital.API.Helpers
{
    public class UserHelper:IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _context;
        private readonly SignInManager<User> _signInManager;

        public UserHelper(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, DataContext context, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager= signInManager;
        }

        public async Task<User> GetUserAsync(string email)
        {
            return await _context.Users
               .Include(x => x.DocumentType)
               .Include(x => x.AccountType)
               .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = roleName });
            }
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async  Task<SignInResult>LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<User> AddUserAsync(UserViewModel model)
        {
            User user = new User
            {
                Address = model.Address,
                Document = model.Document,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ImageFullPath = model.ImageFullPath,
                PhoneNumber = model.PhoneNumber,
                AccountType = await _context.AccountTypes.FindAsync(model.AccountTypeId),
                DocumentType = await _context.DocumentTypes.FindAsync(model.DocumentTypeId),
                CountryCode= model.CountryCode,
                AccountNumber= model.AccountNumber,
                Bank=model.Bank,
                UserType = model.UserType,

            };

            IdentityResult result = await _userManager.CreateAsync(user, "123456");
            if (result != IdentityResult.Success)
            {
                return null;
            }

            User newUser = await GetUserAsync(model.Email);
            await AddUserToRoleAsync(newUser, user.UserType.ToString());
            return newUser;

        }

    }
}
