using EducationTrade.Core.DTOs;
using EducationTrade.Core.Helpers;
using EducationTrade.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EducationTrade.Presentation.Controllers.MVC
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        public AccountController(IAuthService authService, IUserRepository userRepository) 
        {
            _authService = authService;
            _userRepository = userRepository;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
             var result = await _authService.RegisterAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                return View(model);
            }
            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                return View(model);
            }
            HttpContext.Session.SetInt32("UserId", result.Data);
            var user = await _userRepository.GetByIdAsync(result.Data);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);

            TempData["Success"] = $"Welcome back, {user.FullName}!";
            //return View("LogIn");
            return RedirectToAction("Index", "Dashboard");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}
