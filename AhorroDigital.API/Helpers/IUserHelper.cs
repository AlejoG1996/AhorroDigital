using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Models;
using Microsoft.AspNetCore.Identity;

namespace AhorroDigital.API.Helpers
{
    public interface IUserHelper
    {
        Task<User> AddUserAsync(UserViewModel model);
        Task<User> GetUserAsync(string email);
        Task<IdentityResult> AddUserAsync(User user, string password);
        Task CheckRoleAsync(string roleName);
        Task AddUserToRoleAsync(User user, string roleName);
        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();
    }
}
