using Integration.Worker.Models;

namespace Integration.Worker.Services
{
    public interface IMessageProcessor
    {
        Task ProcessAsync(IntegrationMessage integrationMessage, string messageJson, CancellationToken cancellationToken);
    }
}