namespace Integration.Api.Services
{
    public interface IMessageService
    {
        Task<Guid> CreateMessageAsync(IntegrationMessageRequest request);
    }
}