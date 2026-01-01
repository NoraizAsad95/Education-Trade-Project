using EducationTrade.Core.DTOs;
using EducationTrade.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Interfaces
{
    public interface ITaskService
    {
        Task<Result> CreateTaskAsync(CreateTaskDto dto, int creatorId);
        Task<Result> AcceptTaskAsync(int taskId, int accepterId);
        Task<Result> CompleteTaskAsync(int taskId, int creatorId);
        Task<Result<List<Core.Entities.Task>>> GetAvailableTaskAsync();
        Task<Result<List<Core.Entities.Task>>> GetMyCreatedTasksAsync(int userId);
        Task<Result<List<Core.Entities.Task>>> GetMyAcceptedTasksAsync(int userId);
    }
}
