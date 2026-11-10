using FluentAssertions;
using Integration.Api.Controllers;
using Integration.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Integration.Api.Tests.Controllers
{
    public class AdminMessagesControllerTests
    {
        private readonly IReplayMessageService _mockReplayMessageService;
        private readonly AdminMessagesController _controller;

        public AdminMessagesControllerTests()
        {
            _mockReplayMessageService = Substitute.For<IReplayMessageService>();
            _controller = new AdminMessagesController(_mockReplayMessageService);
        }

        /// <summary>
        /// Verifies that GetFailedMessages returns an OK response with the retrieved messages.
        /// </summary>
        [Fact]
        public async Task GetFailedMessages_WithAvailableMessages_ReturnsOkWithMessages()
        {
            // Arrange
            var messages = new List<Data.FailedMessage> { new() { Id = Guid.NewGuid(), Payload = "Test" } };
            _mockReplayMessageService.GetFailedMessagesAsync().Returns(messages);

            // Act
            var result = await _controller.GetFailedMessages();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            result.As<OkObjectResult>().Value.Should().Be(messages);
        }

        /// <summary>
        /// Verifies that GetFailedMessages returns an OK response with empty list when no messages are available.
        /// </summary>
        [Fact]
        public async Task GetFailedMessages_WithNoAvailableMessages_ReturnsOkWithEmptyList()
        {
            // Arrange
            var messages = new List<Data.FailedMessage>();
            _mockReplayMessageService.GetFailedMessagesAsync().Returns(messages);

            // Act
            var result = await _controller.GetFailedMessages();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            result.As<OkObjectResult>().Value.Should().Be(messages);
        }

        /// <summary>
        /// Verifies that GetFailedMessage returns an OK response when a message with the specified ID exists.
        /// </summary>
        [Fact]
        public async Task GetFailedMessage_WithExistingId_ReturnsOkWithMessage()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var message = new Data.FailedMessage { Id = messageId, Payload = "Test Message" };
            _mockReplayMessageService.GetFailedMessageAsync(messageId).Returns(message);

            // Act
            var result = await _controller.GetFailedMessage(messageId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            result.As<OkObjectResult>().Value.Should().Be(message);
        }

        /// <summary>
        /// Verifies that GetFailedMessage returns a NotFound response when the message does not exist.
        /// </summary>
        [Fact]
        public async Task GetFailedMessage_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            _mockReplayMessageService.GetFailedMessageAsync(messageId).Returns((Data.FailedMessage?)null);

            // Act
            var result = await _controller.GetFailedMessage(messageId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        /// <summary>
        /// Verifies that Replay returns an Accepted response with the outbox ID when the message is successfully replayed.
        /// </summary>
        [Fact]
        public async Task Replay_WithExistingId_ReturnsAcceptedWithOutboxId()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var outboxId = Guid.NewGuid();
            _mockReplayMessageService.ReplayAsync(messageId).Returns(outboxId);

            // Act
            var result = await _controller.Replay(messageId);

            // Assert
            result.Should().BeOfType<AcceptedResult>();
            result.As<AcceptedResult>().Value.Should().BeEquivalentTo(new { id = outboxId });
        }

        /// <summary>
        /// Verifies that Replay returns a NotFound response when the message does not exist.
        /// </summary>
        [Fact]
        public async Task Replay_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            _mockReplayMessageService.ReplayAsync(messageId).Returns((Guid?)null);

            // Act
            var result = await _controller.Replay(messageId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        /// <summary>
        /// Verifies that ReplayFixed returns an Accepted response with the outbox ID when the message is successfully replayed with a fixed payload.
        /// </summary>
        [Fact]
        public async Task ReplayFixed_WithExistingIdAndValidPayload_ReturnsAcceptedWithOutboxId()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var outboxId = Guid.NewGuid();
            var payload = "{\"key\": \"value\"}";
            _mockReplayMessageService.ReplayWithPayloadAsync(messageId, payload).Returns(outboxId);

            // Act
            var result = await _controller.ReplayFixed(messageId, payload);

            // Assert
            result.Should().BeOfType<AcceptedResult>();
            result.As<AcceptedResult>().Value.Should().BeEquivalentTo(new { id = outboxId });
        }

        /// <summary>
        /// Verifies that ReplayFixed returns a NotFound response when the message does not exist.
        /// </summary>
        [Fact]
        public async Task ReplayFixed_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var payload = "{\"key\": \"value\"}";
            _mockReplayMessageService.ReplayWithPayloadAsync(messageId, payload).Returns((Guid?)null);

            // Act
            var result = await _controller.ReplayFixed(messageId, payload);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        /// <summary>
        /// Verifies that the controller throws an ArgumentNullException when initialized with a null service.
        /// </summary>
        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Action act = () => new AdminMessagesController(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("replayMessageService");
        }
    }
}