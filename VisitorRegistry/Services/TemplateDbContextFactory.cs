using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using VisitorRegistry.Services;

namespace VisitorRegistry.Data
{
    public class TemplateDbContextFactory : IDesignTimeDbContextFactory<TemplateDbContext>
    {
        public TemplateDbContext CreateDbContext(string[] args)
        {
            // Configurazione dal file appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..\\VisitorRegistry.Web"))
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<TemplateDbContext>();
            builder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));

            return new TemplateDbContext(builder.Options);
        }
    }
}

