using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AhorroDigital.API.Data.Entities
{
    public class AccountType
    {
        public int Id { get; set; }

        [Display(Name = "Tipo de cuenta")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más  de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }
    }
}
