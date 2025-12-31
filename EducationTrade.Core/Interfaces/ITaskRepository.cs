using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<Core.Entities.Task?> GetByIdAsync (int id);
        Task<List<Core.Entities.Task>> GetAvailableTaskAsync(int id);
        Task<List<Core.Entities.Task>> GetTasksByCreatorAsync(int creatorId);
        Task<List<Core.Entities.Task>> GetTasksByAccepterAsync(int accepterId);
        Task<Core.Entities.Task> AddAsync(Core.Entities.Task task);
        Task UpdateAsync(Core.Entities.Task task);
    }
}
