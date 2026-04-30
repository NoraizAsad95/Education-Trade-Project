using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.DTOs
{
    public class CompleteProfileDto
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Please enter your course")]
        public string Course { get; set; }
        [Required(ErrorMessage = "Please enter your specialty")]
        public string Specialty { get; set; }
    }
}
