using System.ComponentModel.DataAnnotations;

namespace AhorroDigital.API.Data.Entities
{
    public class LoanType
    {
        public int Id { get; set; }

        [Display(Name = "Tipo de Prétamo")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más  de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }
    }
}
