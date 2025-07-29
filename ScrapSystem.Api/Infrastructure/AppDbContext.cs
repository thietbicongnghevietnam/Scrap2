using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Domain.Models;

namespace ScrapSystem.Api.Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<ScrapImage> ScrapImages { get; set; }

        public DbSet<Scrap> Scraps { get; set; }

        public DbSet<ScrapDetail> ScrapDetails { get; set; }

        public DbSet<MaterialName> MaterialNames { get; set; }

        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        public DbSet<ScrapViewDto> ScrapViewDtos { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRefreshToken>().HasKey(x=>x.Id);
            modelBuilder.Entity<MaterialName>().HasKey(x => x.Material);


            modelBuilder.Entity<ScrapViewDto>().HasNoKey().ToView(null);

        }

    }
}
