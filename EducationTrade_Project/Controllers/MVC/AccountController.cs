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
            TempData["Success"] = "Registration successful! Please check your email to verify your account.";
            return RedirectToAction("Login");
        }
        // 1. This sends the empty form HTML to the register popup
        [HttpGet]
        public IActionResult RegisterModal()
        {
            return PartialView("_RegisterModal", new RegisterDto());
        }

        // 2. This processes the form when the user clicks "Register"
        [HttpPost]
        public async Task<IActionResult> RegisterModal(RegisterDto model)
        {
            // Scenario A: Form is empty or invalid
            if (!ModelState.IsValid)
            {
                return PartialView("_RegisterModal", model); // Send the form back with errors
            }

            // Scenario B: Registration logic (similar to your existing Register action)
            var result = await _authService.RegisterAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                return PartialView("_RegisterModal", model); // Send the form back with errors
            }

            // Scenario C: Success! Set a TempData message and redirect to the Login page or Modal.
            TempData["Success"] = "Registration successful! Please check your email to verify your account.";

            // Tell HTMX to force the browser to redirect to the Home page or Login
            Response.Headers.Append("HX-Redirect", "/Account/Login");

            return Ok();
        }


        [HttpGet]
        public IActionResult CheckEmail()
        {
            return View();
        }

        
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid verification link";
                return RedirectToAction("Login");
            }

            var result = await _authService.VerifyEmailAsync(email, token);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Email verified successfully! You received 200 coins. Please login to continue.";
            }
            else
            {
                TempData["Error"] = result.Error;
            }

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
        // 1. This just sends the empty form HTML to the popup
        [HttpGet]
        public IActionResult LoginModal()
        {
            return PartialView("_LoginModal", new LoginDto());
        }

        // 2. This processes the form when the user clicks "Submit"
        [HttpPost]
        public async Task<IActionResult> LoginModal(LoginDto model)
        {
            // Scenerio A: Form is empty or invalid
            if (!ModelState.IsValid)
            {
                return PartialView("_LoginModal", model); // Send the form back with errors
            }

            // Scenario B: Wrong email or password
            var result = await _authService.LoginAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                return PartialView("_LoginModal", model); // Send the form back with errors
            }

            // Scenario C: Success! Set up the session.
            HttpContext.Session.SetInt32("UserId", result.Data);
            // (Add the rest of your session logic here...)

            // Tell HTMX to force the browser to redirect to the Dashboard
            Response.Headers.Append("HX-Redirect", "/Dashboard/Index");
            return Ok();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}
