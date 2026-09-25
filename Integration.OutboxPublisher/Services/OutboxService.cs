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

        public async Task<List<OutboxMessage>> GetUnpublishedMessagesAsync(CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var messages = await dbContext.OutboxMessages
                .Where(x => x.PublishedAt == null)
                .ToListAsync(cancellationToken);

            return messages;
        }

        public async Task MarkMessageAsPublishedAsync(Guid messageId, CancellationToken cancellationToken)
        {
            await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var message = await dbContext.OutboxMessages.SingleAsync(x => x.Id == messageId, cancellationToken);

            message.PublishedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
