using Microsoft.EntityFrameworkCore;
using UserService.Models;
namespace UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<UserProfile> Profiles { get; set; }
    }
}