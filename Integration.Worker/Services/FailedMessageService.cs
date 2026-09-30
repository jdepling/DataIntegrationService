using Integration.Data;
using Microsoft.EntityFrameworkCore;

namespace Integration.Worker.Services
{
    public class FailedMessageService : IFailedMessageService
    {
        private readonly IDbContextFactory<IntegrationDbContext> _dbContextFactory;

        public FailedMessageService(IDbContextFactory<IntegrationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task SaveAsync(FailedMessage failedMessage, CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            dbContext.FailedMessages.Add(failedMessage);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}