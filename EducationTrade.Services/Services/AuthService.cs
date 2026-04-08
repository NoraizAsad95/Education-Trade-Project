using EducationTrade.Core.DTOs;
using EducationTrade.Core.Entities;
using EducationTrade.Core.Enums;
using EducationTrade.Core.Helpers;
using EducationTrade.Core.Interfaces;

using EducationTrade.Services.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public AuthService(IUserRepository userRepository, IEmailService emailService, IConfiguration config) 
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _config = config;
        }
        public async Task<Result> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email))
            {
                return Result.Failure("Email already exists.");
            }
            if(string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            {
                return Result.Failure("Password length must be 6 characters");
            }
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                Email = dto.Email,
                Password = PasswordHasher.HashPassword(dto.Password),
                FullName = dto.FullName,
                Course = dto.Course,
                Specialty = dto.Specialty,
                CoinBalance = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsEmailVerified = false,  // Not verified
                EmailVerificationToken = token,
                EmailVerificationTokenExpiry = DateTime.Now.AddHours(24)
            };
            var savedUser = await _userRepository.AddAsync(user);
            // 4. Send verification email
            var baseUrl = _config["AppSettings:BaseUrl"];
            var link = $"{baseUrl}/Account/VerifyEmail?token={token}&email={user.Email}";
            await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, link);
            return Result.Success();
            //return Result.Success(savedUser.UserId);

        }
        public async Task<Result<int>> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                return Result<int>.Failure("Invalid email or password");
            }
            if(!PasswordHasher.VerifyPassword(dto.Password,user.Password))
            {
                return Result<int>.Failure("Invalid email or password");
            }
            if (!user.IsEmailVerified)
            {
                return Result<int>.Failure("Please verify your email before logging in. Check your inbox for verification link.");
            }
            if (!user.IsActive)
            {
                return Result<int>.Failure("Account is deactivated.");
            }
            return Result<int>.Success(user.UserId);
            //return Result.Success(user.UserId);

        }
        public async Task<Result> VerifyEmailAsync(string email, string token)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                return Result.Failure("Invalid verification link");
            }

            if (user.IsEmailVerified)
            {
                return Result.Failure("Email already verified. You can login now!");
            }

            if (user.EmailVerificationToken != token)
            {
                return Result.Failure("Invalid verification token");
            }

            if (user.EmailVerificationTokenExpiry < DateTime.Now)
            {
                return Result.Failure("Verification link has expired. Please request a new one.");
            }

            // Verify email and give initial coins!
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            user.CoinBalance = 200;  
            user.UpdatedAt = DateTime.Now;

            // Transaction Record
            var registrationTrans = new CoinTransaction
            {
                FromUserId = null,  
                ToUserId = user.UserId,
                Coins = 200,
                Type = TransactionType.Registration,
                Description = "Initial signup bonus",
                CreatedAt = DateTime.Now
            };
            await _userRepository.AddCoinTransactionAsync(registrationTrans);
            
            await _userRepository.UpdateAsync(user);

            // Send welcome email
            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
            }
            catch (Exception ex)
            {
                // Welcome email failed but verification succeeded
                Console.WriteLine($"Welcome email failed: {ex.Message}");
            }

            return Result.Success();
        }
        public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                return Result<string>.Success(string.Empty);
            }
            
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.Now.AddHours(1);
            await _userRepository.UpdateAsync(user);
            
            return Result<string>.Success(token);
        }
        public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null ||
                user.PasswordResetToken != dto.Token ||
                user.PasswordResetTokenExpiry == null ||
                user.PasswordResetTokenExpiry < DateTime.Now)
            {
                return Result.Failure("Invalid or expired reset token.");
            }
            
            user.Password = PasswordHasher.HashPassword(dto.NewPassword);

            // Clear the token so it can't be reused
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _userRepository.UpdateAsync(user);
            return Result.Success();
        }


    }
}
