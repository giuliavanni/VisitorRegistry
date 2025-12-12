using System.Threading.Tasks;

namespace VisitorRegistry.Web.SignalR
{
    public interface IPublishDomainEvents
    {
        Task Publish(object evnt);
    }
}
