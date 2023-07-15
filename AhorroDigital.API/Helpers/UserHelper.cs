using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AhorroDigital.API.Models;
using AhorroDigital.Common.Enums;

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
                .Include(x => x.Savings)
                .ThenInclude(x => x.Contributes)
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

        public async Task<User> GetUserAsync(Guid Id)
        {
            return await _context.Users.Include(x => x.DocumentType)
                .Include(x=>x.AccountType)
                .Include(x=>x.Savings)
                .ThenInclude(x=>x.Contributes)
                .FirstOrDefaultAsync(x => x.Id == Id.ToString());
        }

        public async Task<IdentityResult> UpdateUserAsync(User user) {

            User currentUser= await GetUserAsync(user.Email);
            currentUser.LastName= user.LastName;
            currentUser.FirstName= user.FirstName;
            currentUser.Address = user.Address;
            currentUser.CountryCode = user.CountryCode;
            currentUser.AccountType = user.AccountType;
            currentUser.Document = user.Document;
            currentUser.DocumentType = user.DocumentType;
            currentUser.AccountNumber = user.AccountNumber;
            currentUser.Bank = user.Bank;
            currentUser.FirstName = user.FirstName;
            currentUser.PhoneNumber = user.PhoneNumber;
            currentUser.ImageFullPath = user.ImageFullPath;

            return await _userManager.UpdateAsync(currentUser);
        }


       public async Task<IdentityResult> DeleteUserAsync(User user)
        {
            return await _userManager.DeleteAsync(user);
        }

        public async Task<User> AddUserAsync(AddUserViewModel model, UserType userType)
        {
            User user = new User
            {
                Address = model.Address,
                Document = model.Document,
                Email = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CountryCode = model.CountryCode,
                PhoneNumber = model.PhoneNumber,
                DocumentType = await _context.DocumentTypes.FindAsync(model.DocumentTypeId),
                AccountNumber = model.AccountNumber,
                Bank = model.Bank,
                AccountType = await _context.AccountTypes.FindAsync(model.AccountTypeId),
                UserName = model.UserName,
                ImageFullPath=model.ImageFullPath,
                UserType = userType
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            User newUser = await GetUserAsync(model.UserName);
            await AddUserToRoleAsync(newUser, user.UserType.ToString());
            return newUser;

        }
    }
}
