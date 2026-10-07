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

        /// <summary>
        ///     Replays a failed message by its ID. If the message is found, it deserializes the payload, creates a new message in the outbox, and removes the failed message from the database.
        /// </summary>
        /// <param name="failedMessageId">The ID of the failed message to replay.</param>
        /// <returns>The ID of the replayed message, or null if not found.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the failed message cannot be deserialized.</exception>
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

        /// <summary>
        ///    Retrieves all failed messages from the database, ordered by creation date in descending order.
        /// </summary>
        /// <returns>A list of failed messages.</returns>
        public async Task<List<FailedMessage>> GetFailedMessagesAsync()
        {
            return await _dbContext.FailedMessages
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        ///    Retrieves a specific failed message by its ID.
        /// </summary>
        /// <param name="id">The ID of the failed message to retrieve.</param>
        /// <returns>The failed message, or null if not found.</returns>
        public async Task<FailedMessage?> GetFailedMessageAsync(Guid id)
        {
            return await _dbContext.FailedMessages
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        ///   Replays a failed message with the fixed payload. 
        ///   If the message is found, it deserializes the provided payload, creates a new message in the outbox, 
        ///   and removes the failed message from the database.
        /// </summary>
        /// <param name="failedMessageId">The ID of the failed message to replay.</param>
        /// <param name="payload">The fixed payload for the message.</param>
        /// <returns>The ID of the replayed message, or null if not found.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the payload cannot be deserialized.</exception>
        public async Task<Guid?> ReplayWithPayloadAsync(Guid failedMessageId, string payload)
        {
            var failedMessage = await _dbContext.FailedMessages
                .FirstOrDefaultAsync(x => x.Id == failedMessageId);

            if (failedMessage == null)
                return null;

            var request = JsonSerializer.Deserialize<IntegrationMessageRequest>(payload);

            if (request == null)
                throw new InvalidOperationException(
                    $"Unable to deserialize replay payload for failed message {failedMessageId}.");

            var outboxId = await _messageService.CreateMessageAsync(request);

            _dbContext.FailedMessages.Remove(failedMessage);
            await _dbContext.SaveChangesAsync();

            return outboxId;
        }
    }
}
