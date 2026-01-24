using EducationTrade.Core.Interfaces;
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
            ViewBag.User = user;
            ViewBag.MyCreatedTasks = myCreatedTask.Data;
            ViewBag.MyAcceptedTasks = myAcceptedTask.Data;

            return View();
        }
    }

}