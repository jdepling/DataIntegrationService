using System.Text.Json;
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

        /// <summary>
        ///     Creates a new integration message and saves it to the outbox table.
        /// </summary>
        /// <param name="request">The integration message request.</param>
        /// <returns>The ID of the created message.</returns>
        public async Task<Guid> CreateMessageAsync(IntegrationMessageRequest request)
        {
            var message = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                SourceId = request.SourceId,
                Payload = JsonSerializer.Serialize(request),
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.OutboxMessages.Add(message);
            await _dbContext.SaveChangesAsync();

            return message.Id;
        }
    }
}
