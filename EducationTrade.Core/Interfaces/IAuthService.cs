using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EducationTrade.Core.DTOs;
using EducationTrade.Core.Helpers;

namespace EducationTrade.Core.Interfaces
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterDto dto);
        Task<Result> LoginAsync(LoginDto dto);
    }
}
