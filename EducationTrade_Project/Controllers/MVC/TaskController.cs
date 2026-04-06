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
        public async Task<IActionResult> CompleteTask(int id, int score = 5)
        {
            var user = HttpContext.Session.GetInt32("UserId");
            if (user == null)
            {
                return RedirectToAction("LogIn", "Account");
            }
            if (score < 1 || score > 5)
                score = 5;
            var result = await _taskService.CompleteTaskAsync(id, user.Value, score);
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

                var messagesResult = await _taskService.GetTaskMessagesAsync(task.TaskId);
                var messages = messagesResult.IsSuccess ? messagesResult.Data : new List<TaskMessage>();

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
                    
                    CanComplete = task.Status.ToString() == "Accepted",
                    Messages = messages.Select(m => new TaskMessageViewModel
                    {
                        SenderName = m.Sender.FullName,
                        MessageText = m.MessageText,
                        TimeAgo = GetTimeAgo(m.CreatedAt),
                        IsMe = m.SenderId == userId.Value
                    }).ToList()
                });
            }

            // Map accepted tasks
            foreach (var task in acceptedResult.Data)
            {
                var creator = await _userRepository.GetByIdAsync(task.CreatedById);

                var messagesResult = await _taskService.GetTaskMessagesAsync(task.TaskId);
                var messages = messagesResult.IsSuccess ? messagesResult.Data : new List<TaskMessage>();

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
                    Messages = messages.Select(m => new TaskMessageViewModel
                    {
                        SenderName = m.Sender.FullName,
                        MessageText = m.MessageText,
                        TimeAgo = GetTimeAgo(m.CreatedAt),
                        IsMe = m.SenderId == userId.Value
                    }).ToList()
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

        [HttpPost]
        public async Task<IActionResult> CancelTask(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("LogIn", "Account");

            var result = await _taskService.CancelTaskAsync(id, userId.Value);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "Task cancelled successfully! Coins have been refunded to your wallet.";
            }

            return RedirectToAction("MyTasks");
        }

        [HttpPost]
        public async Task<IActionResult> DropTask(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("LogIn", "Account");

            var result = await _taskService.DropTaskAsync(id, userId.Value);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "You have dropped the task. It is now available for others.";
            }

            return RedirectToAction("MyTasks");
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage(int taskId, string messageText, string returnTab)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null && !string.IsNullOrWhiteSpace(messageText))
            {
                await _taskService.AddMessageAsync(taskId, userId.Value, messageText);
            }
            // Return to the page and open the specific tab (Created or Accepted)
            TempData["ActiveTab"] = returnTab;
            return RedirectToAction("MyTasks");
        }

        [HttpPost]
        public async Task<IActionResult> DeliverWork(int taskId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                await _taskService.SubmitForReviewAsync(taskId, userId.Value);
                TempData["Success"] = "Work submitted for review successfully!";
            }
            TempData["ActiveTab"] = "accepted-tab";
            return RedirectToAction("MyTasks");
        }

        [HttpPost]
        public async Task<IActionResult> RequestRevision(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("LogIn", "Account");

            var result = await _taskService.RequestRevisionAsync(id, userId.Value);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
            }
            else
            {
                TempData["Success"] = "You have requested a revision from the worker.";
            }

            return RedirectToAction("MyTasks");
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
