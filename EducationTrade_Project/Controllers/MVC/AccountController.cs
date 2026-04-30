using EducationTrade.Core.DTOs;
using EducationTrade.Core.Helpers;
using EducationTrade.Core.Interfaces;
using EducationTrade.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace EducationTrade.Presentation.Controllers.MVC
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        public AccountController(IAuthService authService, IUserRepository userRepository, IEmailService emailService) 
        {
            _authService = authService;
            _userRepository = userRepository;
            _emailService = emailService;
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _authService.ForgotPasswordAsync(model);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
               var token = result.Data; 
               var resetLink = Url.Action("ResetPassword", "Account",
                    new { email = model.Email, token = token },
                    protocol: Request.Scheme);
                await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink);
            }
            TempData["Success"] = "If an account with that email exists, we have sent a password reset link.";    
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }
            var model = new ResetPasswordDto { Email = email, Token = token };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _authService.ResetPasswordAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                return View(model);
            }
            TempData["Success"] = "Your password has been reset successfully. Please log in.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = "/Dashboard/Index")
        {
            // This tells ASP.NET Core to challenge the Google/GitHub scheme
            var redirectUrl = Url.Action("OAuthCallback", "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);   // provider = "Google" or "GitHub"
        }
        [HttpGet]
        public async Task<IActionResult> OAuthCallback(string returnUrl = "/Dashboard/Index")
        {
            // Read what Google/GitHub sent back
            var result = await HttpContext.AuthenticateAsync("Cookies");
            if (!result.Succeeded)
            {
                TempData["Error"] = "OAuth login failed. Please try again.";
                return RedirectToAction("LogIn");
            }
            // Extract user info from the OAuth claims
            var claims = result.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var fullName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var provider = result.Properties.Items[".AuthScheme"] ?? "Unknown";
            // The unique ID this user has on that provider
            var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerId))
            {
                TempData["Error"] = "Could not retrieve your information from the provider.";
                return RedirectToAction("LogIn");
            }
            // Hand off to your AuthService (finds or creates the user in YOUR DB)
            var loginResult = await _authService.OAuthLoginAsync(provider, providerId, email, fullName ?? email);
            if (!loginResult.IsSuccess)
            {
                TempData["Error"] = loginResult.Error;
                return RedirectToAction("LogIn");
            }
            // Set up the same session you use for normal login
            var userId = loginResult.Data;
            var user = await _userRepository.GetByIdAsync(userId);
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            if (string.IsNullOrEmpty(user.Course) || string.IsNullOrEmpty(user.Specialty))
            {
                TempData["Info"] = "Welcome! Please complete your profile to continue.";
                return RedirectToAction("CompleteProfile");
            }
            TempData["Success"] = $"Welcome, {user.FullName}! Logged in via {provider}.";
            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> CompleteProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("LogIn");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("LogIn");

            // Already complete → skip this page
            if (!string.IsNullOrEmpty(user.Course) && !string.IsNullOrEmpty(user.Specialty))
                return RedirectToAction("Index", "Dashboard");

            var model = new CompleteProfileDto { FullName = user.FullName };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteProfile(CompleteProfileDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("LogIn");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("LogIn");

            user.FullName = model.FullName;
            user.Course = model.Course;
            user.Specialty = model.Specialty;
            await _userRepository.UpdateAsync(user);

            HttpContext.Session.SetString("UserName", user.FullName);

            TempData["Success"] = "Profile completed! Welcome to EducationTrade.";
            return RedirectToAction("Index", "Dashboard");
        }

    }
}
