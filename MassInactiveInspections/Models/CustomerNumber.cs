using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MassInactiveInspections.Models
{
    public class CustomerNumber
    {
        [Required]
        [Display(Name = "Customer Number:" )]
        public int? customerNum { get; set; }
    }
}