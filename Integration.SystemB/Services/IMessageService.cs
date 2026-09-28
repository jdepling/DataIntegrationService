using Integration.SystemB.Models;

namespace Integration.SystemB.Services
{
    public interface IMessageService
    {
        Task<MessageResponse> CreateAsync(
            CreateMessageRequest request,
            CancellationToken cancellationToken);
    }
}