using EducationTrade.Core.DTOs;
using EducationTrade.Core.Entities;
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

        public async Task<Result> CompleteTaskAsync(int taskId, int creatorId, int ratingScore)
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
            if (task.Status != TaskState.Accepted && task.Status != TaskState.PendingReview)
            {
                return Result.Failure("Task must be accepted before completion");
            }

            // 4. Transfer coins to accepter
            var accepter = await _userRepository.GetByIdAsync(task.AcceptedById.Value);
            accepter.CoinBalance += task.CoinReward;

            if (ratingScore >= 1 && ratingScore <= 5)
            {
                decimal currentTotalScore = accepter.AverageRating * accepter.TotalRatings;
                decimal newTotalScore = currentTotalScore + ratingScore;

                accepter.TotalRatings += 1;
                accepter.AverageRating = newTotalScore / accepter.TotalRatings;
                // Create the History Record
                var rating = new Rating
                {
                    TaskId = taskId,
                    RatedUserId = accepter.UserId,
                    RatedByUserId = creatorId,
                    Score = ratingScore,
                    CreatedAt = DateTime.Now
                };
                await _userRepository.AddRatingAsync(rating);
            }

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
        public async Task<Result> CancelTaskAsync(int taskId, int creatorId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null) return Result.Failure("Task not found");

            // Security Check: Only the Creator can cancel the task
            if (task.CreatedById != creatorId) return Result.Failure("Only the task creator can cancel this task");

            // Status Check: You can only cancel a task if it hasn't been accepted yet
            if (task.Status != TaskState.Pending) return Result.Failure("You cannot cancel a task that is already accepted or completed.");

            // Refund the Coins back to the Creator
            var creator = await _userRepository.GetByIdAsync(creatorId);
            creator.CoinBalance += task.CoinReward;
            await _userRepository.UpdateAsync(creator);

            // Update the Task Status
            task.Status = TaskState.Cancelled;
            await _taskRepository.UpdateAsync(task);

            return Result.Success();
        }
        public async Task<Result> DropTaskAsync(int taskId, int assigneeId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null) return Result.Failure("Task not found");

            
            if (task.AcceptedById != assigneeId) return Result.Failure("Only the assigned worker can drop this task");

            
            if (task.Status != TaskState.Accepted) return Result.Failure("Task is not in an active working state");

            
            task.AcceptedById = null;
            task.Status = TaskState.Pending;
            await _taskRepository.UpdateAsync(task);

            return Result.Success();
        }


        public async Task<Result> AddMessageAsync(int taskId, int senderId, string text)
        {
            var message = new TaskMessage
            {
                TaskId = taskId,
                SenderId = senderId,
                MessageText = text,
                CreatedAt = DateTime.Now
            };
            await _taskRepository.AddMessageAsync(message);
            return Result.Success();
        }

        public async Task<Result> SubmitForReviewAsync(int taskId, int assigneeId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.AcceptedById != assigneeId) return Result.Failure("Invalid request");

            task.Status = TaskState.PendingReview;
            await _taskRepository.UpdateAsync(task);
            return Result.Success();
        }

        public async Task<Result> RequestRevisionAsync(int taskId, int creatorId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);

            if(task == null || task.CreatedById != creatorId) return Result.Failure("Invalid request");

            if (task.Status != TaskState.PendingReview) return Result.Failure("Task is not under review");

            task.Status = TaskState.Accepted;
            await _taskRepository.UpdateAsync(task);

            var systemMessage = new TaskMessage
            {
                TaskId = taskId,
                SenderId = creatorId,
                CreatedAt = DateTime.Now,
                MessageText = "SYSTEM:  The Creator has requested revisions. Please check the chat and resubmit your work."
            };
            await _taskRepository.AddMessageAsync(systemMessage);

            return Result.Success();

        }

        public async Task<Result<List<TaskMessage>>> GetTaskMessagesAsync(int taskId)
        {
            var messages = await _taskRepository.GetMessagesByTaskIdAsync(taskId);
            return Result<List<TaskMessage>>.Success(messages);
        }

    }
}
