using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.DTOs
{
    public class CreateTaskDto
    {
        public string Title {  get; set; }
        public string Description { get; set; }
        public int CoinReward { get; set; }
    }
}
