using EducationTrade.Core.DTOs;
using EducationTrade.Core.Entities;
using EducationTrade.Core.Helpers;
using EducationTrade.Core.Interfaces;

using EducationTrade.Services.Helpers;
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
        public AuthService(IUserRepository userRepository) 
        {
            _userRepository = userRepository;
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
            var user = new User
            {
                Email = dto.Email,
                Password = PasswordHasher.HashPassword(dto.Password),
                FullName = dto.FullName,
                Course = dto.Course,
                Specialty = dto.Specialty,
                CoinBalance = 200,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            var savedUser = await _userRepository.AddAsync(user);

                return Result.Success();
            //return Result.Success(savedUser.UserId);

        }
        public async Task<Result> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                return Result.Failure("Invalid email or password");
            }
            if(!PasswordHasher.VerifyPassword(dto.Password,user.Password))
            {
                return Result.Failure("Invalid email or password");
            }
            if(!user.IsActive)
            {
                return Result.Failure("Account is deactivated.");
            }
            return Result.Success();
            //return Result.Success(user.UserId);

        }


    }
}
