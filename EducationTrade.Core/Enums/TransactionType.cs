using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Enums
{
    public enum TransactionType
    {
        Registration = 0,      // Initial 200 coins when user signs up
        TaskCreation = 1,      // Coins locked when creating task
        TaskCompletion = 2,    // Coins transferred when task completed
        Refund = 3            // Coins returned if task cancelled
    }

}
