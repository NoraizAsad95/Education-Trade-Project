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
        public int TaskId { get; set; }
        public int RatedUserId { get; set; }
        public int RatedByUserId { get; set; }

        [Range(1, 5)]
        public int Score { get; set; } // 1 to 5
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
