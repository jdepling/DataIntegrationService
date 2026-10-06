using Integration.Data;
using Microsoft.EntityFrameworkCore;

namespace Integration.OutboxPublisher.Services
{
    public class OutboxService : IOutboxService
    {
        private readonly IDbContextFactory<IntegrationDbContext> _dbContextFactory;

        public OutboxService(IDbContextFactory<IntegrationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        ///     Gets all unpublished messages from the outbox.
        /// </summary>
        /// <returns>A list of unpublished messages.<see cref="OutboxMessage"/></returns>
        public async Task<List<OutboxMessage>> GetUnpublishedMessagesAsync(CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var messages = await dbContext.OutboxMessages
                .Where(x => x.PublishedAt == null)
                .ToListAsync(cancellationToken);

            return messages;
        }

        /// <summary>
        ///    Marks a message as published by setting its PublishedAt property to the current UTC time.
        /// </summary>
        /// <param name="messageId">The ID of the message to mark as published.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        public async Task MarkMessageAsPublishedAsync(Guid messageId, CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var message = await dbContext.OutboxMessages.SingleAsync(x => x.Id == messageId, cancellationToken);

            message.PublishedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
