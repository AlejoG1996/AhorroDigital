﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace AhorroDigital.API.Data.Entities
{
    public class Saving
    {
        public int Id { get; set; }

        [Display(Name="Tipo de Ahorro")]
        [Required(ErrorMessage ="El campo {0} es obligatorio.")]
        public SavingType SavingType { get; set; }

        [Display(Name = "Fecha Inicio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DateIni { get; set; }

        [Display(Name = "Fecha Fin")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DateEnd =>DateIni.AddDays(200);

        [Display(Name = "Valor Minimo Ahorro")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayFormat(DataFormatString ="{0:C2}")]
        public int MinValue { get; set; }

        [Display(Name = "Propietario")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public User User { get; set; }

        [Display(Name = "Observación")]
        [MaxLength(150, ErrorMessage = "El campo {0} no puede tener más  de {1} carácteres.")]
        [DataType(DataType.MultilineText)]
        public string Marks { get; set; }

        public ICollection<Contribute> Contributes { get; set; }

        [Display(Name = "# Aportes")]
        public int ContributesCount => Contributes == null ? 0 : Contributes.Count;

        [Display(Name ="Total Aprobado")]
        public int Total => Contributes== null? 0 : 
            Contributes.Sum(x=>x.ValueAvail);

        [Display(Name = "Total Pendiente")]
        public int TotalOut=> Contributes == null ? 0 :
           Contributes.Sum(x => x.Value);

      

    }
}