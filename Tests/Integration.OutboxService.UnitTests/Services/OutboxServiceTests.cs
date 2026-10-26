using FluentAssertions;
using Integration.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Integration.OutboxPublisher.UnitTests.Services;

/// <summary>
///     Contains unit tests for the <see cref="OutboxService"/>, verifying message retrieval and publishing behavior using an in-memory
///     database
/// </summary>
public class OutboxServiceTests
{
    private static PooledDbContextFactory<IntegrationDbContext> CreateDbContextFactory()
    {
        var options = new DbContextOptionsBuilder<IntegrationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PooledDbContextFactory<IntegrationDbContext>(options);
    }

    [Fact]
    public async Task GetUnpublishedMessagesAsync_WhenMessagesExist_ReturnsUnpublishedMessages()
    {
        // Arrange
        var factory = CreateDbContextFactory();

        var unpublishedMessages = new List<OutboxMessage>
        {
            new() { Id = Guid.NewGuid(), PublishedAt = null },
            new() { Id = Guid.NewGuid(), PublishedAt = null }
        };

        await using (var dbContext = await factory.CreateDbContextAsync())
        {
            dbContext.OutboxMessages.AddRange(unpublishedMessages);

            dbContext.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                PublishedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }

        var service = new OutboxPublisher.Services.OutboxService(factory);

        // Act
        var result = await service.GetUnpublishedMessagesAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.PublishedAt == null);
        result.Should().BeEquivalentTo(unpublishedMessages);
    }

    [Fact]
    public async Task MarkMessageAsPublishedAsync_WhenMessageExists_SetsPublishedAtAndSaves()
    {
        // Arrange
        var factory = CreateDbContextFactory();

        var messageId = Guid.NewGuid();

        await using (var dbContext = await factory.CreateDbContextAsync())
        {
            dbContext.OutboxMessages.Add(new OutboxMessage
            {
                Id = messageId,
                PublishedAt = null
            });

            await dbContext.SaveChangesAsync();
        }

        var service = new OutboxPublisher.Services.OutboxService(factory);

        // Act
        await service.MarkMessageAsPublishedAsync(messageId, CancellationToken.None);

        // Assert
        await using var verificationContext =
            await factory.CreateDbContextAsync();

        var updatedMessage = await verificationContext.OutboxMessages
            .SingleAsync(x => x.Id == messageId);

        updatedMessage.PublishedAt.Should().NotBeNull();
    }
}
