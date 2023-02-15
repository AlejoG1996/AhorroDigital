using AhorroDigital.API.Data;
using AhorroDigital.API.Data.Entities;
using AhorroDigital.API.Models;

namespace AhorroDigital.API.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;

        public ConverterHelper(DataContext context, ICombosHelper combosHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
        }

        public async Task<User> ToUserAsync(UserViewModel model, bool isNew)
        {
            return new User
            {
                Address = model.Address,
                CountryCode = model.CountryCode,
                Document = model.Document,
                DocumentType = await _context.DocumentTypes.FindAsync(model.DocumentTypeId),
                Email = model.Email,
                FirstName = model.FirstName,
                Id = isNew ? Guid.NewGuid().ToString() : model.Id,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Email,
                UserType = model.UserType,
                AccountType = await _context.AccountTypes.FindAsync(model.AccountTypeId),
                AccountNumber = model.AccountNumber,
                Bank = model.Bank,
                ImageFullPath = model.ImageFullPath
                

            };
        }

        public UserViewModel ToUserViewModel(User user)
        {
            return new UserViewModel
            {
                Address = user.Address,
                CountryCode = user.CountryCode,
                Document = user.Document,
                DocumentTypeId = user.DocumentType.Id,
                DocumentTypes = _combosHelper.GetComboDocumentTypes(),
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType,
                AccountTypeId = user.AccountType.Id,
                AccountTypes = _combosHelper.GetComboAccountTypes(),
                AccountNumber=user.AccountNumber, 
                Bank = user.Bank,
                ImageFullPath=user.ImageFullPath
            };
        }

        public async Task<Saving> ToSavingAsync(SavingViewModel model, bool isNew)
        {
            return new Saving
            {
                SavingType = await _context.SavingTypes.FindAsync(model.SavingTypeId),
                DateIni = model.DateIni,
                MinValue = model.MinValue,
                Id = isNew ? 0 : model.Id,
                Marks = model.Marks
            };
        }

       public  SavingViewModel ToSavingViewModel(Saving saving)
        {
            return new SavingViewModel
            {
                SavingTypes = _combosHelper.GetComboSavingTypes(),
                SavingTypeId=saving.SavingType.Id,
                Id = saving.Id,
                DateIni = saving.DateIni,
                MinValue = saving.MinValue,
                Marks = saving.Marks,
                UserId = saving.User.Id
            };
        }

      
    }
}
