using EducationTrade.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Entities
{
    public class CoinTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int? FromUserId { get; set; }
        public int ToUserId { get; set; }

        public int Coins { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
       
    }
}
