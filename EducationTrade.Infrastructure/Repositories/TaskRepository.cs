using EducationTrade.Core.Entities;
using EducationTrade.Core.Interfaces;
using EducationTrade.Infrastructure.Data;
using System;
using EducationTrade.Core.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EducationTrade.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Core.Entities.Task> AddAsync(Core.Entities.Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<List<Core.Entities.Task>> GetAvailableTaskAsync(int id)
        {
                return await _context.Tasks
                .Where(t => t.Status == TaskState.Pending)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Core.Entities.Task?> GetByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<List<Core.Entities.Task>> GetTasksByAccepterAsync(int accepterId)
        {
            return await _context.Tasks
                .Where(t => t.AcceptedById == accepterId)
                .ToListAsync();
        }

        public async Task<List<Core.Entities.Task>> GetTasksByCreatorAsync(int creatorId)
        {
            return await _context.Tasks
                .Where(t => t.CreatedById == creatorId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task UpdateAsync(Core.Entities.Task task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

       
    }
}
