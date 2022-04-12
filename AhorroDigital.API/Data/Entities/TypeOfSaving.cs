using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AhorroDigital.API.Data.Entities
{
    public class TypeOfSaving
    {
        public int Id { get; set; }

        [Display(Name = "Tipo de Ahorro")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Description { get; set; }

        
    }
}
