using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Models.ViewModels
{
    public class CustomerRegistrationViewModel
    {
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
