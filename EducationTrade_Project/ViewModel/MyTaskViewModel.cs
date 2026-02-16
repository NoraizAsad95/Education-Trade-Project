using System.Collections.Generic;

namespace EducationTrade.Web.ViewModels
{
    public class MyTasksViewModel
    {
        public List<MyTaskItemViewModel> CreatedTasks { get; set; } = new();
        public List<MyTaskItemViewModel> AcceptedTasks { get; set; } = new();

        public int TotalCreated { get; set; }
        public int TotalAccepted { get; set; }
        public int CoinsSpent { get; set; }
        public int CoinsEarned { get; set; }
    }


    public class MyTaskItemViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CoinReward { get; set; }
        public string Status { get; set; }

        // Other User Info
        public string OtherUserName { get; set; }  // Creator if I accepted, Accepter if I created
        public decimal OtherUserRating { get; set; }

        // Dates
        public DateTime CreatedAt { get; set; }
      

        // Actions
        public bool CanComplete { get; set; }
        public bool CanRate { get; set; }
    }
}