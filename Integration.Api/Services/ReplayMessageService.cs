using System.Text.Json;
using Integration.Data;
using Microsoft.EntityFrameworkCore;

namespace Integration.Api.Services
{
    public class ReplayMessageService : IReplayMessageService
    {
        private readonly IntegrationDbContext _dbContext;
        private readonly IMessageService _messageService;

        public ReplayMessageService(IntegrationDbContext dbContext, IMessageService messageService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        public async Task<Guid?> ReplayAsync(Guid failedMessageId)
        {
            var failedMessage = await _dbContext.FailedMessages
                .FirstOrDefaultAsync(x => x.Id == failedMessageId);

            if (failedMessage == null)
                return null;

            var request = JsonSerializer.Deserialize<IntegrationMessageRequest>(
                failedMessage.Payload);

            if (request == null)
                throw new InvalidOperationException(
                    $"Unable to deserialize failed message {failedMessageId}.");

            var outboxId = await _messageService.CreateMessageAsync(request);

            _dbContext.FailedMessages.Remove(failedMessage);
            await _dbContext.SaveChangesAsync();

            return outboxId;
        }

        public async Task<List<FailedMessage>> GetFailedMessagesAsync()
        {
            return await _dbContext.FailedMessages
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
