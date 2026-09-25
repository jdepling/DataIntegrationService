using Integration.Data;

namespace Integration.Api.Services
{
    public class MessageService: IMessageService
    {
        private readonly IntegrationDbContext _dbContext;
        public MessageService(IntegrationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Guid> CreateMessageAsync(IntegrationMessageRequest request)
        {
            var message = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                SourceId = request.SourceId,
                Payload = request.Data.GetRawText(),
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.OutboxMessages.Add(message);
            await _dbContext.SaveChangesAsync();

            return message.Id;
        }
    }
}
