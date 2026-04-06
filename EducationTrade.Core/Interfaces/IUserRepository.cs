using EducationTrade.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?>  GetByIdAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        System.Threading.Tasks.Task UpdateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<Rating> AddRatingAsync(Rating rating);


    }
}
