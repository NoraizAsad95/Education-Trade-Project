using EducationTrade.Core.DTOs;
using EducationTrade.Core.Interfaces;
using EducationTrade.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationTrade.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]

    public class ApiTaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IUserRepository _userRepository;

        public ApiTaskController(ITaskService taskService, IUserRepository userRepository)
        {
            _taskService = taskService;
            _userRepository = userRepository;
        }
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableTask()
        {
            var result = await _taskService.GetAvailableTaskAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(new { message = result.Data });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask(CreateTaskDto dto, int creatorId)
        {
            var user = await _userRepository.GetByIdAsync(creatorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _taskService.CreateTaskAsync(dto, creatorId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok("Task Created Successfully");
        }

        [HttpGet("{taskId}/accept")]

        public async Task<IActionResult> AcceptTask(int taskId, int acceptorId)
        {
            var result = await _taskService.AcceptTaskAsync(taskId, acceptorId);

            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(new { message = "Task accepted successfully" });
        }

        [HttpGet("{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(int taskId, int creatorId, int ratingScore)
        {
            var result = await _taskService.CompleteTaskAsync(taskId, creatorId, ratingScore);
            if(!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(new { message = "Task completed successfully" });

        }

        [HttpGet("{userId}/my-created")]
        public async Task<IActionResult> GetMyCreatedTasks(int userId)
        {
            var result = await _taskService.GetMyCreatedTasksAsync(userId);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }
            return Ok(new
            {
                message = "My created tasks retrieved", result.Data
            });
        }

        [HttpGet("{userId}/my-accepted")]
        public async Task<IActionResult> GetMyAcceptedTasks(int userId)
        {
            var result = await _taskService.GetMyAcceptedTasksAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            return Ok(new { message = "My accepted tasks retrieved", result.Data });
        }
    }
}




















































// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties


// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

//-----[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

//-----[assembly: Guid("ca4f9e78-25b4-4883-a5b1-8c0f82fe5257")]
