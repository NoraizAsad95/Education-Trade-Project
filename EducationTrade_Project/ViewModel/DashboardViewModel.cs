using EducationTrade.Core.Entities;


namespace EducationTrade.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }
        public int CoinBalance { get; set; }

        public List<Core.Entities.Task> MyCreatedTasks { get; set; } = new();
        public List<Core.Entities.Task> MyAcceptedTasks { get; set;} = new();

        public int TotalTasksCreated => MyCreatedTasks.Count;
        public int TotalTasksAccepted => MyAcceptedTasks.Count;
        public int CoinsSpent => MyCreatedTasks.Sum(t => t.CoinReward);
        public int CoinsEarned => MyAcceptedTasks
            .Where(t => t.Status == Core.Enums.TaskState.Completed)
            .Sum(t => t.CoinReward);
    }
}