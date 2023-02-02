using AhorroDigital.API.Data.Entities;

namespace AhorroDigital.API.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;

        public SeedDb(DataContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckAccountTypesAsync();
            await CheckDocumentTypesAsync();
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
