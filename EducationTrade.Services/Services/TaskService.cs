using EducationTrade.Core.DTOs;
using EducationTrade.Core.Enums;
using EducationTrade.Core.Helpers;
using EducationTrade.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Services.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        public TaskService (ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task<Result> AcceptTaskAsync(int taskId, int accepterId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return Result.Failure("Task not found");
            }

            // 2. Validate task status
            if (task.Status != TaskState.Pending)
            {
                return Result.Failure("Task is not available");
            }

            // 3. Can't accept own task
            if (task.CreatedById == accepterId)
            {
                return Result.Failure("You cannot accept your own task");
            }
            // 4. Update task
            task.AcceptedById = accepterId;
            task.Status = TaskState.Accepted;
            await _taskRepository.UpdateAsync(task);

            return Result.Success();
        }

        public async Task<Result> CompleteTaskAsync(int taskId, int creatorId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return Result.Failure("Task not found");
            }

            // 2. Validate creator
            if (task.CreatedById != creatorId)
            {
                return Result.Failure("Only task creator can mark as complete");
            }

            // 3. Validate task status
            if (task.Status != TaskState.Accepted)
            {
                return Result.Failure("Task must be accepted before completion");
            }

            // 4. Transfer coins to accepter
            var accepter = await _userRepository.GetByIdAsync(task.AcceptedById.Value);
            accepter.CoinBalance += task.CoinReward;
            await _userRepository.UpdateAsync(accepter);

            // 5. Update task status
            task.Status = TaskState.Completed;
            
            await _taskRepository.UpdateAsync(task);

            return Result.Success();
        }

        public async Task<Result> CreateTaskAsync(CreateTaskDto dto, int CreatedById)
        {
            if(string.IsNullOrWhiteSpace(dto.Title))
            {
                return Result.Failure("Title is required");
            }
            if(dto.CoinReward <=0 )
            {
                return Result.Failure("Coin Reward must be greater then 0");
            }
            var creator = await _userRepository.GetByIdAsync(CreatedById);
            if (creator == null)
            {
                return Result.Failure("User not found");
            }

            if (creator.CoinBalance < dto.CoinReward)
            {
                return Result.Failure($"Insufficient coins. You have {creator.CoinBalance}, need {dto.CoinReward}");
            }
            creator.CoinBalance -= dto.CoinReward;
            await _userRepository.UpdateAsync(creator);
            var task = new Core.Entities.Task()
            { 
                Title = dto.Title,
                Description = dto.Description,
                CoinReward = dto.CoinReward,
                Status = TaskState.Pending,
                CreatedAt = DateTime.Now,
                CreatedById = CreatedById
            };
            var savedTask = await _taskRepository.AddAsync(task);
            
            return Result.Success();
            //return Result.Success(savedTask.TaskId);

        }

        public async Task<Result<List<Core.Entities.Task>>> GetAvailableTaskAsync()
        {
            var tasks = await _taskRepository.GetAvailableTaskAsync();
            return Result<List<Core.Entities.Task>>.Success(tasks);
        }


        public async Task<Result<List<Core.Entities.Task>>> GetMyAcceptedTasksAsync(int userId)
        {
            var tasks = await _taskRepository.GetTasksByAccepterAsync(userId);
            return Result<List<Core.Entities.Task>>.Success(tasks);
        }

        public async Task<Result<List<Core.Entities.Task>>> GetMyCreatedTasksAsync(int userId)
        {
            var tasks = await _taskRepository.GetTasksByCreatorAsync(userId);
            return Result<List<Core.Entities.Task>>.Success(tasks);
        }
    }
}
