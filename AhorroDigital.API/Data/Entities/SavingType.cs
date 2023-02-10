using System.ComponentModel.DataAnnotations;

namespace AhorroDigital.API.Data.Entities
{
    public class SavingType
    {
        public int Id { get; set; }

        [Display(Name = "Tipo de Ahorro")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más  de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        public ICollection<Saving> Savings { get; set; }
    }
}
