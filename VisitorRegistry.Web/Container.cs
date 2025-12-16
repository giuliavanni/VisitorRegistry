using Microsoft.Extensions.DependencyInjection;
using VisitorRegistry.Services.Shared;
using VisitorRegistry.Web.SignalR;

namespace VisitorRegistry.Web
{
    public class Container
    {
        public static void RegisterTypes(IServiceCollection container)
        {
            // Registration of all the database services you have
            container.AddScoped<SharedService>();

            // Registration of SignalR events
            container.AddScoped<IPublishDomainEvents, SignalrPublishDomainEvents>();
        }
    }
}
