using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using AMS.Models;

namespace AMS.Data
{
    public class ApiDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<APIModel> ApiModels { get; set; }
        public DbSet<BoughtAPIs> BoughtApis { get; set; }
        public DbSet<UserWallet> wallets { get; set; }

        public ApiDbContext() { }
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Application User
            modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Wallet).WithOne(w => w.User).HasForeignKey<UserWallet>(w => w.UserID)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.RefreshTokens).WithOne(t => t.User).HasForeignKey("UserID")
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.BoughtApis).WithOne(a => a.User).HasForeignKey(u => u.UserID)
            .OnDelete(DeleteBehavior.Cascade);

            //User Wallet
            modelBuilder.Entity<UserWallet>()
            .HasKey(w => w.id);
            modelBuilder.Entity<UserWallet>()
            .Property(w => w.id)
            .ValueGeneratedOnAdd();
            modelBuilder.Entity<UserWallet>()
            .HasOne(w => w.User).WithOne(u => u.Wallet).OnDelete(DeleteBehavior.Restrict);

            //Refresh Token
            modelBuilder.Entity<RefreshToken>()
            .HasKey(t => t.Token);
            modelBuilder.Entity<RefreshToken>()
            .Property(t => t.Token)
            .HasMaxLength(32);


            /// API Model
            //ID
            modelBuilder.Entity<APIModel>()
            .HasKey(a => a.id);
            //Name
            modelBuilder.Entity<APIModel>()
            .Property(a => a.name)
            .IsRequired()
            .HasMaxLength(20);
            //URL
            modelBuilder.Entity<APIModel>()
            .Property(a => a.url)
            .IsRequired()
            .HasMaxLength(100);
            modelBuilder.Entity<APIModel>()
            .HasIndex(a => a.url)
            .IsUnique();
            //Sample Call
            modelBuilder.Entity<APIModel>()
            .Property(a => a.sampleCall)
            .HasMaxLength(100);
            //Description
            modelBuilder.Entity<APIModel>()
            .Property(a => a.desc)
            .HasMaxLength(200);
            //Price
            modelBuilder.Entity<APIModel>()
            .Property(a => a.price)
            .IsRequired();

            //Bought APIs
            modelBuilder.Entity<BoughtAPIs>()
            .HasKey(b => new { b.apiID, b.UserID });
            modelBuilder.Entity<BoughtAPIs>()
            .HasOne(b => b.User).WithMany(u => u.BoughtApis).HasForeignKey(b => b.UserID)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BoughtAPIs>()
            .HasOne(b => b.api).WithMany(a => a.boughtAPIs).HasForeignKey(b => b.apiID)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}