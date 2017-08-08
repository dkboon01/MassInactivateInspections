using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MassInactiveInspections.Models
{
    public class CustomerNumber
    {
        [Display(Name = "Customer Number:" )]
        [Required(ErrorMessage = "Customer Number is Required")]
        [Range(1, 999999, ErrorMessage = "Customer Number is Required")]
        public int? customerNum { get; set; }
    }

    public class CustomerNumberList
    {
        [Required]
        [Display(Name = "Customer Number List:")]
        public List<CustomerNumber> customerNumberList { get; set; }
    }

    public class JSONCustomerNumber
    {
        [Required]
        [Display(Name = "Customer Number:")]
        public int? customerNum { get; set; }
        public bool isSelected { get; set; }
    }

    public class jSONCustomerNumberList
    {
        [Required]
        [Display(Name = "Customer Number List:")]
        public List<JSONCustomerNumber> JSONCustomerNumberLists { get; set; }
    }
}