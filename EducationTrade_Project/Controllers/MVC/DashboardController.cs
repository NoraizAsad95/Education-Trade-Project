using EducationTrade.Core.Interfaces;
using EducationTrade.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;


namespace EducationTrade.Presentation.Controllers.MVC
{
    public class DashboardController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskService _taskService;

        public DashboardController(IUserRepository userRepository, ITaskService taskService)
        {
            _userRepository = userRepository;
            _taskService = taskService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if(userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userRepository.GetByIdAsync(userId.Value);
            var myCreatedTask = await _taskService.GetMyCreatedTasksAsync(userId.Value);
            var myAcceptedTask = await _taskService.GetMyAcceptedTasksAsync(userId.Value);
            var viewModel = new DashboardViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                CoinBalance = user.CoinBalance, 
                MyAcceptedTasks = myAcceptedTask.Data,
                MyCreatedTasks = myCreatedTask.Data
            };

            return View(viewModel);
        }
    }

}