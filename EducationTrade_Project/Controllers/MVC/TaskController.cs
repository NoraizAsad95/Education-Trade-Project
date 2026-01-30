using EducationTrade.Core.DTOs;
using EducationTrade.Core.Interfaces;
using EducationTrade.Presentation.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;

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
            if(userId == null)
            {
                return RedirectToAction("LogIn", "Account");
            }
            var user = await _userRepository.GetByIdAsync(userId.Value);
            var result = await _taskService.GetAvailableTaskAsync();

            var viewModel = new TaskViewModel()
            {
                currentUserId = userId.Value,
                Tasks = result.Data,
                UserCoinBalance = user.CoinBalance
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTask()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskDto Model)
        {
            var user = HttpContext.Session.GetInt32("UserId");
            if(user == null)
            {
                return RedirectToAction("LogIn", "Account");
            }
            if (!ModelState.IsValid)
            {
                return View(Model);
            }
            var result = await _taskService.CreateTaskAsync(Model,user.Value);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                return View(Model);
            }

            TempData["Success"] = "Task created successfully!";
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
                TempData["Success"] = "Task accepted successfully!";
            }
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
