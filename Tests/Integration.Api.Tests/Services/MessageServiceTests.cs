using System.Text.Json;
using FluentAssertions;
using Integration.Api.Services;
using Integration.Data;
using Microsoft.EntityFrameworkCore;

namespace Integration.Api.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="MessageService"/>
    /// </summary>
    public class MessageServiceTests
    {
        private readonly IntegrationDbContext _dbContext;
        private readonly MessageService _sut;

        public MessageServiceTests()
        {
            var options = new DbContextOptionsBuilder<IntegrationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new IntegrationDbContext(options);
            _sut = new MessageService(_dbContext);
        }

        /// <summary>
        /// Verifies that CreateMessageAsync persists the message to the database and returns a valid ID.
        /// This is the happy path test ensuring core functionality works.
        /// </summary>
        [Fact]
        public async Task CreateMessageAsync_ValidRequest_SavesMessageAndReturnsId()
        {
            // Arrange
            var request = new IntegrationMessageRequest
            {
                SourceId = "TEST-001",
                Data = JsonSerializer.SerializeToElement(new { key = "value" })
            };

            // Act
            var actualId = await _sut.CreateMessageAsync(request);

            // Assert
            actualId.Should().NotBeEmpty();
            _dbContext.OutboxMessages.Should().ContainSingle(x => x.Id == actualId);
        }

        /// <summary>
        /// Verifies that the request data is properly serialized to JSON in the payload.
        /// This ensures no data loss occurs during persistence.
        /// </summary>
        [Fact]
        public async Task CreateMessageAsync_ValidRequest_SerializesRequestToPayload()
        {
            // Arrange
            var request = new IntegrationMessageRequest
            {
                SourceId = "TEST-002",
                Data = JsonSerializer.SerializeToElement(new { customerId = 123 })
            };

            // Act
            await _sut.CreateMessageAsync(request);

            // Assert
            var savedMessage = _dbContext.OutboxMessages.First();
            savedMessage.Payload.Should().Contain(request.SourceId).And.Contain("customerId");
        }

        /// <summary>
        /// Verifies that outbox messages are timestamped with UTC time.
        /// This ensures audit trail integrity and proper event ordering.
        /// </summary>
        [Fact]
        public async Task CreateMessageAsync_ValidRequest_SetsCreatedAtToUtcNow()
        {
            // Arrange
            var request = new IntegrationMessageRequest
            {
                SourceId = "TEST-003",
                Data = JsonSerializer.SerializeToElement(new { })
            };
            var beforeCall = DateTime.UtcNow;

            // Act
            await _sut.CreateMessageAsync(request);
            var afterCall = DateTime.UtcNow;

            // Assert
            var savedMessage = _dbContext.OutboxMessages.First();
            savedMessage.CreatedAt.Should().BeOnOrAfter(beforeCall).And.BeOnOrBefore(afterCall);
        }

        /// <summary>
        /// Verifies that each message receives a unique identifier.
        /// This prevents message collision and duplication issues.
        /// </summary>
        [Fact]
        public async Task CreateMessageAsync_MultipleRequests_GeneratesUniqueIdForEachMessage()
        {
            // Arrange
            var request = new IntegrationMessageRequest
            {
                SourceId = "TEST-004",
                Data = JsonSerializer.SerializeToElement(new { })
            };

            // Act
            var id1 = await _sut.CreateMessageAsync(request);
            var id2 = await _sut.CreateMessageAsync(request);

            // Assert
            id1.Should().NotBe(id2);
            _dbContext.OutboxMessages.Should().HaveCount(2);
        }

        /// <summary>
        /// Verifies that null requests are rejected.
        /// This prevents null reference exceptions and ensures data validation.
        /// </summary>
        [Fact]
        public async Task CreateMessageAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(x => x.CreateMessageAsync(null!))
                .Should().ThrowAsync<ArgumentNullException>();
        }
    }
}