using EducationTrade.Core.Entities;

namespace EducationTrade.Presentation.ViewModel
{
    public class TaskViewModel
    {
        public int currentUserId { get; set; }
        public List<TaskItemViewModel> AvailableTasks { get; set; } = new();
        public int UserCoinBalance { get; set; }
        public string UserName { get; set; }
    }
    public class TaskItemViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CoinReward { get; set; }
        public string Status { get; set; }
        public int CreatedById { get; set; }
        public string CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; }
    }

 }
