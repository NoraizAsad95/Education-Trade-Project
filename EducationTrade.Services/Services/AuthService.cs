using EducationTrade.Core.DTOs;
using EducationTrade.Core.Entities;
using EducationTrade.Core.Enums;
using EducationTrade.Core.Helpers;
using EducationTrade.Core.Interfaces;
using EducationTrade.Services.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
        public async Task<Result<string>> LoginApiAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.Password))
                return Result<string>.Failure("Invalid email or password");
            if (!user.IsEmailVerified)
                return Result<string>.Failure("Please verify your email first.");
            if (!user.IsActive)
                return Result<string>.Failure("Account is deactivated.");

            // Build JWT Claims
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FullName)
    };

            // Build JWT Token
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return Result<string>.Success(new JwtSecurityTokenHandler().WriteToken(token));
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
        public async Task<Result<int>> OAuthLoginAsync(string provider, string providerId, string email, string fullName)
        {
            //  Try to find user by their OAuth provider ID (most reliable — doesn't change)
            var user = await _userRepository.GetByOAuthProviderAsync(provider, providerId);

            if (user != null)
            {
                // User already exists via OAuth → just log them in
                if (!user.IsActive)
                    return Result<int>.Failure("Account is deactivated.");

                return Result<int>.Success(user.UserId);
            }

            //  Check if a manual account with the same email exists
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                // Link OAuth to the existing manual account
                existingUser.OAuthProvider = provider;
                existingUser.OAuthProviderId = providerId;
                existingUser.IsOAuthUser = true;
                existingUser.IsEmailVerified = true; // Google/GitHub already verified their email
                await _userRepository.UpdateAsync(existingUser);

                return Result<int>.Success(existingUser.UserId);
            }

            //  Brand new user via OAuth → auto-register them
            var newUser = new User
            {
                Email = email,
                FullName = fullName,
                Password = string.Empty,           // No password for OAuth users
                IsEmailVerified = true,            // Provider already verified
                IsOAuthUser = true,
                OAuthProvider = provider,
                OAuthProviderId = providerId,
                IsActive = true,
                CoinBalance = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var savedUser = await _userRepository.AddAsync(newUser);

            // Give them the 200 coin signup bonus
            var registrationTrans = new CoinTransaction
            {
                FromUserId = null,
                ToUserId = savedUser.UserId,
                Coins = 200,
                Type = TransactionType.Registration,
                Description = "Initial signup bonus (OAuth)",
                CreatedAt = DateTime.Now
            };
            await _userRepository.AddCoinTransactionAsync(registrationTrans);
            savedUser.CoinBalance = 200;
            await _userRepository.UpdateAsync(savedUser);

            // Send welcome email 
            try
            {
                await _emailService.SendWelcomeEmailAsync(savedUser.Email, savedUser.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Welcome email failed: {ex.Message}");
            }

            return Result<int>.Success(savedUser.UserId);
        }


    }
}
