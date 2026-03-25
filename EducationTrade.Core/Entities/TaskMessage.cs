using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Entities
{
    public class TaskMessage
    {
        [Key]
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int SenderId { get; set; }
        
        public string MessageText { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("TaskId")]
        public Task Task { get; set; }
        [ForeignKey("SenderId")]
        public User Sender { get; set; }
    }
}
