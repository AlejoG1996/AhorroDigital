﻿using System.ComponentModel.DataAnnotations;

namespace AhorroDigital.API.Data.Entities
{
    public class SavingType
    {
        public int Id { get; set; }

        [Display(Name = "Tipo de Ahorro")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más  de {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string? Name { get; set; }

        public ICollection<User>? Users { get; set; }
        public ICollection<Saving>? Savings { get; set; }


        [Display(Name = "Vr. Minimo Ahorro")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public int MinValue { get; set; }


        [Display(Name = "Número de registros")]
        public int NumberRegister { get; set; }
    }
}
