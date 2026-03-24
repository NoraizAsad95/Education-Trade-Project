using EducationTrade.Core.DTOs;
using EducationTrade.Core.Entities;
using EducationTrade.Core.Interfaces;
using EducationTrade.Presentation.ViewModel;
using EducationTrade.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Collections.Generic;

namespace EducationTrade.Presentation.Controllers.MVC
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserRepository _userRepository;
        public TaskController(ITaskService taskService, IUserRepository userRepository)
        {
            _taskService = taskService;
            _userRepository = userRepository;
        }
        public async Task<IActionResult> Browse()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            var result = await _taskService.GetAvailableTaskAsync();

            // Create ViewModel
            var viewModel = new TaskViewModel
            {
                currentUserId = userId.Value,
                UserCoinBalance = user.CoinBalance,
                UserName = user.FullName,
                AvailableTasks = new List<TaskItemViewModel>()
            };

            // Map each task to TaskItemViewModel
            foreach (var task in result.Data)
            {
                var creator = await _userRepository.GetByIdAsync(task.CreatedById);

                viewModel.AvailableTasks.Add(new TaskItemViewModel
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    Description = task.Description,
                    CoinReward = task.CoinReward,
                    Status = task.Status.ToString(),
                    CreatedById = task.CreatedById,
                    CreatorName = creator.FullName,
                    
                    CreatedAt = task.CreatedAt,
                    TimeAgo = GetTimeAgo(task.CreatedAt),
                    //CanAccept = task.CreatedById != userId.Value,  // Can't accept own tasks
                   // StatusBadgeClass = GetStatusBadgeClass(task.Status.ToString())
                });
            }

            return View(viewModel);
        }


        
        public async Task<IActionResult> CreateTask()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            var viewModel = new CreateTaskViewModel()
            {
                UserName = user.FullName,
                UserCoinBalance = user.CoinBalance
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null) return RedirectToAction("Login", "Account");

                viewModel.UserCoinBalance = user.CoinBalance;
                viewModel.UserName = user.FullName;
                return View("CreateTask", viewModel);
            }

            var dto = new CreateTaskDto
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                CoinReward = viewModel.CoinReward
            };

            var result = await _taskService.CreateTaskAsync(dto, userId.Value);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null) return RedirectToAction("Login", "Account");

                viewModel.UserCoinBalance = user.CoinBalance;
                viewModel.UserName = user.FullName;
                return View("CreateTask", viewModel);
            }

            TempData["Success"] = "Task created successfully! Your coins have been locked.";
            return RedirectToAction("Browse");
        }
        [HttpPost]
        public async Task<IActionResult> AcceptTask(int id)
        {
            var user = HttpContext.Session.GetInt32("UserId");
            if (user == null)
            {
                return RedirectToAction("LogIn", "Account");
            }
            var result = await _taskService.AcceptTaskAsync(id, user.Value);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Task accepted successfully!";
            }
            return RedirectToAction("Browse");
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTask(int id)
        {
            var user = HttpContext.Session.GetInt32("UserId");
            if (user == null)
            {
                return RedirectToAction("LogIn", "Account");
            }
            var result = await _taskService.CompleteTaskAsync(id, user.Value);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Task marked as complete! Coins transferred to accepter.";
            }
            return RedirectToAction("MyTasks", "Task");
        }

            // GET: /Task/MyTasks
        public async Task<IActionResult> MyTasks()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var createdResult = await _taskService.GetMyCreatedTasksAsync(userId.Value);
            var acceptedResult = await _taskService.GetMyAcceptedTasksAsync(userId.Value);

            var viewModel = new MyTasksViewModel
            {
                CreatedTasks = new List<MyTaskItemViewModel>(),
                AcceptedTasks = new List<MyTaskItemViewModel>()
            };

            // Map created tasks
            foreach (var task in createdResult.Data)
            {
                string otherUserName = "Waiting for acceptance...";
                decimal otherUserRating = 0;

                if (task.AcceptedById.HasValue)
                {
                    var accepter = await _userRepository.GetByIdAsync(task.AcceptedById.Value);
                    otherUserName = accepter.FullName;
                    otherUserRating = accepter.AverageRating;
                }

                viewModel.CreatedTasks.Add(new MyTaskItemViewModel
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    Description = task.Description,
                    CoinReward = task.CoinReward,
                    Status = task.Status.ToString(),
                    OtherUserName = otherUserName,
                    OtherUserRating = otherUserRating,
                    CreatedAt = task.CreatedAt,
                    
                    CanComplete = task.Status.ToString() == "Accepted"
                });
            }

            // Map accepted tasks
            foreach (var task in acceptedResult.Data)
            {
                var creator = await _userRepository.GetByIdAsync(task.CreatedById);

                viewModel.AcceptedTasks.Add(new MyTaskItemViewModel
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    Description = task.Description,
                    CoinReward = task.CoinReward,
                    Status = task.Status.ToString(),
                    OtherUserName = creator.FullName,
                    OtherUserRating = creator.AverageRating,
                    CreatedAt = task.CreatedAt,
                   
                });
            }

            // Calculate statistics
            viewModel.TotalCreated = viewModel.CreatedTasks.Count;
            viewModel.TotalAccepted = viewModel.AcceptedTasks.Count;
            viewModel.CoinsSpent = viewModel.CreatedTasks.Sum(t => t.CoinReward);
            viewModel.CoinsEarned = viewModel.AcceptedTasks
                .Where(t => t.Status == "Completed")
                .Sum(t => t.CoinReward);

            return View(viewModel);
        }
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";

            return dateTime.ToString("MMM dd, yyyy");
        }
    }
}
