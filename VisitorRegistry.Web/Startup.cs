//using VisitorRegistry.Web.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VisitorRegistry.Services;
using VisitorRegistry.Services.Visitors;
using VisitorRegistry.Web.Infrastructure;
using VisitorRegistry.Web.SignalR.Hubs;


namespace VisitorRegistry.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Env = env;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddScoped<VisitorService>();

            services.AddScoped<PresenceService>();

            services.AddDbContext<TemplateDbContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });

            // SERVICES FOR AUTHENTICATION
            services.AddSession();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Login/Login";
                options.LogoutPath = "/Login/Logout";
            });

            var builder = services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {                        // Enable loading SharedResource for ModelLocalizer
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

#if DEBUG
            builder.AddRazorRuntimeCompilation();
#endif

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.AreaViewLocationFormats.Clear();
                options.AreaViewLocationFormats.Add("/Areas/{2}/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");

                options.ViewLocationFormats.Clear();
                options.ViewLocationFormats.Add("/Features/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Features/Views/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Features/Views/Shared/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            });

            // SIGNALR FOR COLLABORATIVE PAGES
            services.AddSignalR();

            // CONTAINER FOR ALL EXTRA CUSTOM SERVICES
            Container.RegisterTypes(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Mostra errori completi
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // Https redirection only in production
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Localization support if you want to
            app.UseRequestLocalization(SupportedCultures.CultureNames);

            app.UseRouting();

            // Adding authentication to pipeline
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();




            // Cartelle da usare come provider
            var baseFolder = Directory.GetCurrentDirectory();
            var relativeFolders = new[] { "wwwroot", "node_modules", "Areas" };

            var providers = new List<IFileProvider>();

            foreach (var folder in relativeFolders)
            {
                var fullPath = Path.Combine(baseFolder, folder);

                // Se la cartella non esiste, la crea
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                providers.Add(new PhysicalFileProvider(fullPath));
            }

            // Aggiungi anche il WebRoot preesistente
            providers.Add(env.WebRootFileProvider);

            // Combina tutto
            var compositeProvider = new CompositeFileProvider(providers);

            // Imposta il WebRootFileProvider
            env.WebRootFileProvider = compositeProvider;

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TemplateHub>("/templateHub");

                // Default route
                //endpoints.MapControllerRoute("default", "{controller=Login}/{action=Login}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                // Route area Example
                endpoints.MapAreaControllerRoute("Example", "Example", "Example/{controller=Users}/{action=Index}/{id?}");

                // Route per VisitorController
                endpoints.MapControllerRoute("visitor", "{controller=Visitor}/{action=Index}/{id?}");

            });

        }
    }

    public static class SupportedCultures
    {
        public readonly static string[] CultureNames;
        public readonly static CultureInfo[] Cultures;

        static SupportedCultures()
        {
            CultureNames = new[] { "it-it" };
            Cultures = CultureNames.Select(c => new CultureInfo(c)).ToArray();

            //NB: attenzione nel progetto a settare correttamente <NeutralLanguage>it-IT</NeutralLanguage>
        }
    }
}
