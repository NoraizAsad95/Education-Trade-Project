using EducationTrade.Core.Entities;
using EducationTrade.Core.Interfaces;
using EducationTrade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User> AddAsync(User user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.Where(u => u.IsActive).ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async System.Threading.Tasks.Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task<Rating> AddRatingAsync(Rating rating)
        {
            _context.Add(rating);
            await _context.SaveChangesAsync();
            return rating;
        }

        public async Task<CoinTransaction> AddCoinTransactionAsync(CoinTransaction transaction)
        {
            _context.CoinTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }
        public async Task<User?> GetByOAuthProviderAsync(string provider, string providerId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.OAuthProvider == provider && u.OAuthProviderId == providerId);
        }

    }
}
