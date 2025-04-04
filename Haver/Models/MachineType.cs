﻿using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class MachineType : Auditable
    {
        public int ID { get; set; }


        // Machine Description

        //#region SUMMARY PROPERTY
        
        //public string Description { get {
        //        return Class + "-" + Size + " " + Deck;
            
        //    }
        //}

        //#endregion


        [Display(Name = "Machine Model")]
        [Required(ErrorMessage = "Machine Model is required.")]
        public string? Description {  get; set; }


        //[Display(Name = "Machine Size")]
        //[Required(ErrorMessage = "Machine size is required.")]
        //public string? Size {  get; set; }


        //[Display(Name = "Machine Deck")]
        //[Required(ErrorMessage = "Machine deck is required.")]
        //public string? Deck {  get; set; }

        public ICollection<Machine> Machines { get; set; } = new HashSet<Machine>();
    }
}
