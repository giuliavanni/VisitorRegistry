using Microsoft.EntityFrameworkCore;
using VisitorRegistry.Infrastructure;
using VisitorRegistry.Services.Shared;

namespace VisitorRegistry.Services
{
    public class TemplateDbContext : DbContext
    {
        public TemplateDbContext()
        {
        }

        public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
        {
            DataGenerator.InitializeUsers(this);
        }

        public DbSet<User> Users { get; set; }
    }
}
