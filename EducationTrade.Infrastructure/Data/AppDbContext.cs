using EducationTrade.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Infrastructure.Data
{
     public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Core.Entities.Task> Tasks { get; set; }
        public DbSet<CoinTransaction> CoinTransactions { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.AverageRating)
          .HasPrecision(3, 2); // e.g. 4.50 max
            });
            //modelBuilder.Entity<User>()
            //   .Property(u => u.CoinBalance)
            //   .HasDefaultValue(200);
            modelBuilder.Entity<Core.Entities.Task>(entity =>
            {
                entity.Property(e => e.CoinReward).IsRequired();
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById);
            });
            modelBuilder.Entity<CoinTransaction>(entity =>
            {
                
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ToUserId);
            });
        }
    }
    
}
