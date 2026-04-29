using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EducationTrade.Core.DTOs;
using EducationTrade.Core.Helpers;
using System.Security.Claims;
namespace EducationTrade.Core.Interfaces
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterDto dto);
        Task<Result<int>> LoginAsync(LoginDto dto);
        Task<Result<string>> LoginApiAsync(LoginDto dto);
        Task<Result> VerifyEmailAsync(string email, string token);
        Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
        Task<Result<int>> OAuthLoginAsync(string provider, string providerId, string email, string fullName);
    }
}
