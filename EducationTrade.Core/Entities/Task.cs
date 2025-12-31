using EducationTrade.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Entities
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CreatedById { get; set; }
        public int? AcceptedById { get; set; }

        public int CoinReward { get; set; }
        
        public TaskState Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
