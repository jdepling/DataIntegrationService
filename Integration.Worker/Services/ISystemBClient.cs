using Integration.Worker.Models;

namespace Integration.Worker.Services
{
    public interface ISystemBClient
    {
        Task SendAsync(SystemBMessageRequest request, CancellationToken cancellationToken);
    }
}