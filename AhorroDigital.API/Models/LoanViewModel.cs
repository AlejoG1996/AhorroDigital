using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AhorroDigital.API.Models
{
    public class LoanViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tipo de Préstamo")]
        [Range(1, int.MaxValue, ErrorMessage = "debes seleccionar un tipo de préstamo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int LoanTypeId { get; set; }

        public IEnumerable<SelectListItem>? LoanTypes { get; set; }

        [Display(Name = "Fecha Solicitud")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DateS { get; set; }

        [Display(Name = "Valor Préstamo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public int Value { get; set; }

        [Display(Name = "Valor disponible")]
      [DisplayFormat(DataFormatString = "{0:C2}")]
        public int? ValueAvail { get; set; }

        [Display(Name = "Interes")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public double Interest { get; set; }

        [Display(Name = "# Cuotas")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int Dues { get; set; }

        public string? UserId { get; set; }

        [Display(Name = "Observación")]
        [MaxLength(150, ErrorMessage = "El campo {0} no puede tener más  de {1} carácteres.")]
        [DataType(DataType.MultilineText)]
        public string? Marks { get; set; }

        [Display(Name = "Foto")]

        public IFormFile? ImageFile { get; set; }
        [Display(Name = "Estado")]
        public string? State { get; set; }

        [Display(Name = "Foto")]
        public string ImageFullPath { get; set; }
    }
}
