//namespace ScrapSystem.Web.Data
//{
//    public class AppDbContext
//    {
//    }
//}

using Microsoft.EntityFrameworkCore;
using ScrapSystem.Web.Models; // chứa MaterSection

namespace ScrapSystem.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<MaterSection> MaterSections { get; set; }
        public DbSet<Users> Users { get; set; }

    }
}