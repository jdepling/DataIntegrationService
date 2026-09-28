using Integration.SystemB.Data;
using Integration.SystemB.Models;
using Microsoft.EntityFrameworkCore;

namespace Integration.SystemB.Services
{
    public class MessageService : IMessageService
    {
        private readonly SystemBDbContext _dbContext;

        public MessageService(SystemBDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MessageResponse> CreateAsync(CreateMessageRequest request, CancellationToken cancellationToken)
        {

            var existingMessage = await _dbContext
                .Messages
                .FirstOrDefaultAsync(x => x.SourceId == request.SourceId, cancellationToken);

            if (existingMessage != null)
            {
                return new MessageResponse
                {
                    Id = existingMessage.Id,
                    SourceId = existingMessage.SourceId,
                    CustomerId = existingMessage.CustomerId,
                    Name = existingMessage.Name,
                    Amount = existingMessage.Amount,
                    Status = existingMessage.Status,
                    CreatedAt = existingMessage.CreatedAt
                };
            }

            var message = new Message
            {
                SourceId = request.SourceId,
                CustomerId = request.CustomerId,
                Name = request.Name,
                Amount = request.Amount,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new MessageResponse
            {
                Id = message.Id,
                SourceId = message.SourceId,
                CustomerId = message.CustomerId,
                Name = message.Name,
                Amount = message.Amount,
                Status = message.Status,
                CreatedAt = message.CreatedAt
            };
        }
    }
}