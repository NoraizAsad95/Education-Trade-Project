using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Entities
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        public string RatedUserId { get; set; }
        public string RatedByUserId { get; set; }

        public int Score { get; set; } // 1 to 5
    }
}
