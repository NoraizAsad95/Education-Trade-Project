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
                return RedirectToAction("Login", "Account");
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

    }
}
