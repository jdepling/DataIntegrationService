using FluentAssertions;
using Integration.Data;
using Integration.OutboxPublisher.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Integration.OutboxPublisher.UnitTests.Services
{
    public class OutboxMessageProcessorTests
    {
        private readonly IOutboxService _outboxService = Substitute.For<IOutboxService>();
        private readonly IPublishService _publishService = Substitute.For<IPublishService>();
        private readonly ILogger<OutboxMessageProcessor> _logger = Substitute.For<ILogger<OutboxMessageProcessor>>();

        private OutboxMessageProcessor CreateProcessor()
        {
            return new OutboxMessageProcessor(_outboxService, _publishService, _logger);
        }

        /// <summary>
        ///     Verifies that every unpublished message is published and marked as published.
        /// </summary>
        [Fact]
        public async Task ProcessMessagesAsync_WhenMessagesExist_PublishesAndMarksEachMessage()
        {
            // Arrange
            var firstMessage = new OutboxMessage { Id = Guid.NewGuid(), SourceId = "source-1", Payload = """{"id":1}""" };
            var secondMessage = new OutboxMessage { Id = Guid.NewGuid(), SourceId = "source-2", Payload = """{"id":2}""" };
            var messages = new List<OutboxMessage> { firstMessage, secondMessage };
            using var cts = new CancellationTokenSource();

            _outboxService.GetUnpublishedMessagesAsync(cts.Token).Returns(messages);

            var processor = CreateProcessor();

            // Act
            await processor.ProcessMessagesAsync(cts.Token);

            // Assert
            await _publishService.Received(1).PublishAsync(firstMessage, cts.Token);
            await _publishService.Received(1).PublishAsync(secondMessage, cts.Token);
            await _outboxService.Received(1).MarkMessageAsPublishedAsync(firstMessage.Id, cts.Token);
            await _outboxService.Received(1).MarkMessageAsPublishedAsync(secondMessage.Id, cts.Token);
        }

        /// <summary>
        ///     Verifies that no publishing or database updates occur when the outbox is empty.
        /// </summary>
        [Fact]
        public async Task ProcessMessagesAsync_WhenNoMessagesExist_DoesNotPublishAnything()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            _outboxService.GetUnpublishedMessagesAsync(cts.Token).Returns(new List<OutboxMessage>());

            var processor = CreateProcessor();

            // Act
            await processor.ProcessMessagesAsync(cts.Token);

            // Assert
            await _publishService.DidNotReceive().PublishAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
            await _outboxService.DidNotReceive().MarkMessageAsPublishedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        ///     Verifies that a message is not marked as published when publishing fails.
        /// </summary>
        [Fact]
        public async Task ProcessMessagesAsync_WhenPublishingFails_DoesNotMarkMessageAsPublished()
        {
            // Arrange
            var message = new OutboxMessage { Id = Guid.NewGuid(), SourceId = "source-1", Payload = """{"id":1}""" };
            using var cts = new CancellationTokenSource();

            _outboxService.GetUnpublishedMessagesAsync(cts.Token).Returns(new List<OutboxMessage> { message });
            _publishService.PublishAsync(message, cts.Token).Returns(_ => Task.FromException(new InvalidOperationException("RabbitMQ unavailable")));

            var processor = CreateProcessor();

            // Act
            var act = () => processor.ProcessMessagesAsync(cts.Token);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("RabbitMQ unavailable");
            await _outboxService.DidNotReceive().MarkMessageAsPublishedAsync(message.Id, cts.Token);
        }
    }
}
