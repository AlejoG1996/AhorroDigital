using AhorroDigital.Common.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AhorroDigital.API.Data.Entities
{
    public class User:IdentityUser
    {
        [Display(Name = "Nombres")]
        [MaxLength(50, ErrorMessage = "El campo {0}  no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string FirstName { get; set; }

        [Display(Name = "Apellidos")]
        [MaxLength(50, ErrorMessage = "El campo {0}  no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string LastName { get; set; }

        [Display(Name = "Tipo de Documento")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public DocumentType DocumentType { get; set; }


        [Display(Name = "Documento")]
        [MaxLength(20, ErrorMessage = "El campo {0}  no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Document { get; set; }

        [Display(Name = "Dirección")]
        [MaxLength(150, ErrorMessage = "El campo {0}  no puede tener más de {1} carácteres.")]
        public string Address { get; set; }


        [DefaultValue("57")]
        [Display(Name = "Codigo País")]
        [MaxLength(5, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string CountryCode { get; set; }

        [Display(Name = "Tipo de cuenta bancaria")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public AccountType AccountType { get; set; }

        [Display(Name = "Número cuenta bancaria")]
        [MaxLength(20, ErrorMessage = "El campo {0}  no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string AccountNumber { get; set; }

        [Display(Name = "Banco")]
        [MaxLength(40, ErrorMessage = "El campo {0}  no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Bank { get; set; }

        [Display(Name = "Foto")]
        public Guid ImageId { get; set; }

        [Display(Name = "Foto")]
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"http://localhost:65014/images/noimages.png"
            : $"http://localhost:65014/images/{ImageId}";

        [Display(Name = "Usuario")]
        public UserType UserType { get; set; }

        [Display(Name = "Usuario")]
        public string FullName => $"{FirstName} {LastName}";

    }
}
