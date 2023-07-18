using Microsoft.EntityFrameworkCore;
using WebApplication1.Modal;

namespace WebApplication1.Data
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
        }

        public DbSet<User> User { get; set; }
    }
}
