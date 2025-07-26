using System.Threading;
using System.Threading.Tasks;
using Blazing.Mediator;

namespace Blazing.Mediator.Benchmarks
{
    public class Pinged : INotification
    {
    }

    public class PingedHandler : INotificationSubscriber<Pinged>
    {
        public Task OnNotification(Pinged notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}