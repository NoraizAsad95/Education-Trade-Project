using EducationTrade.Core.Entities;

namespace EducationTrade.Presentation.ViewModel
{
    public class TaskViewModel
    {
        public int currentUserId { get; set; }
        public List<Core.Entities.Task> Tasks { get; set; }
        public int UserCoinBalance { get; set; }
    }
}
