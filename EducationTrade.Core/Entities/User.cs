using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }  
        public string FullName { get; set; }
        public string? Course { get; set; }       
        public string? Specialty { get; set; }

        public int CoinBalance { get; set; } 

        public decimal AverageRating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;

        // Account Status
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ✅ NEW: Email Verification Fields
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
    }
}
