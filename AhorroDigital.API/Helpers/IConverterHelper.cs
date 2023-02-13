using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Models;

namespace AhorroDigital.API.Helpers
{
    public interface IConverterHelper
    {

        Task<User> ToUserAsync(UserViewModel model,  bool isNew);
        UserViewModel ToUserViewModel(User user);

        Task<Saving>ToSavingAsync(SavingViewModel model, bool isNew);
        SavingViewModel ToSavingViewModel(Saving saving);
    }
}
