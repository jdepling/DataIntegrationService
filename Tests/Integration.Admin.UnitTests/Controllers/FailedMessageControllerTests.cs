using FluentAssertions;
using Integration.Admin.Controllers;
using Integration.Admin.Models;
using Integration.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Integration.Admin.UnitTests.Controllers
{
    public class FailedMessageControllerTests
    {
        private readonly IAdminApiClient _adminApiClient;
        private readonly FailedMessagesController _controller;

        public FailedMessageControllerTests()
        {
            _adminApiClient = Substitute.For<IAdminApiClient>();
            _controller = new FailedMessagesController(_adminApiClient);
        }

        #region Details Tests

        /// <summary>
        ///     Returns View result with message when message exists
        /// </summary>
        [Fact]
        public async Task Details_MessageExists_ReturnsViewResultWithMessage()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var failedMessage = new FailedMessageViewModel { Id = messageId, Payload = "Test message" };
            _adminApiClient.GetFailedMessageAsync(messageId).Returns(failedMessage);

            // Act
            var result = await _controller.Details(messageId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(failedMessage);
        }

        /// <summary>
        ///     Returns NotFound result when message does not exist
        /// </summary>
        [Fact]
        public async Task Details_MessageNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            _adminApiClient.GetFailedMessageAsync(messageId).Returns((FailedMessageViewModel?)null);

            // Act
            var result = await _controller.Details(messageId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        /// <summary>
        ///     Calls GetFailedMessageAsync with correct message ID
        /// </summary>
        [Fact]
        public async Task Details_WhenCalled_CallsGetFailedMessageAsyncWithCorrectId()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            _adminApiClient.GetFailedMessageAsync(messageId).Returns((FailedMessageViewModel?)null);

            // Act
            await _controller.Details(messageId);

            // Assert
            await _adminApiClient.Received(1).GetFailedMessageAsync(messageId);
        }

        #endregion

        #region Index Tests

        /// <summary>
        ///     Returns View result with list of messages
        /// </summary>
        [Fact]
        public async Task Index_WhenCalled_ReturnsViewResultWithMessages()
        {
            // Arrange
            var messages = new List<FailedMessageViewModel>
            {
                new() { Id = Guid.NewGuid(), Payload = "Message 1" },
                new() { Id = Guid.NewGuid(), Payload = "Message 2" }
            };
            _adminApiClient.GetFailedMessagesAsync().Returns(messages);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(messages);
        }

        /// <summary>
        ///     Calls GetFailedMessagesAsync once
        /// </summary>
        [Fact]
        public async Task Index_WhenCalled_CallsGetFailedMessagesAsyncOnce()
        {
            // Arrange
            _adminApiClient.GetFailedMessagesAsync().Returns([]);

            // Act
            await _controller.Index();

            // Assert
            await _adminApiClient.Received(1).GetFailedMessagesAsync();
        }

        /// <summary>
        ///     Returns View result with empty list when no messages exist
        /// </summary>
        [Fact]
        public async Task Index_NoMessages_ReturnsViewResultWithEmptyList()
        {
            // Arrange
            var emptyMessages = new List<FailedMessageViewModel>();
            _adminApiClient.GetFailedMessagesAsync().Returns(emptyMessages);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(emptyMessages);
        }

        #endregion

        #region Replay Tests

        /// <summary>
        ///     Returns RedirectToAction to Index when replay succeeds
        /// </summary>
        [Fact]
        public async Task Replay_ReplaySucceeds_ReturnsRedirectToActionToIndex()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            _adminApiClient.ReplayMessageAsync(messageId).Returns(true);

            // Act
            var result = await _controller.Replay(messageId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be(nameof(FailedMessagesController.Index));
        }

        /// <summary>
        ///     Returns NotFound result when replay fails
        /// </summary>
        [Fact]
        public async Task Replay_ReplayFails_ReturnsNotFoundResult()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            _adminApiClient.ReplayMessageAsync(messageId).Returns(false);

            // Act
            var result = await _controller.Replay(messageId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        /// <summary>
        ///     Calls ReplayMessageAsync with correct message ID
        /// </summary>
        [Fact]
        public async Task Replay_WhenCalled_CallsReplayMessageAsyncWithCorrectId()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            _adminApiClient.ReplayMessageAsync(messageId).Returns(false);

            // Act
            await _controller.Replay(messageId);

            // Assert
            await _adminApiClient.Received(1).ReplayMessageAsync(messageId);
        }

        #endregion

        #region ReplayFixed Tests

        /// <summary>
        ///     Returns RedirectToAction to Index when replay with fixed payload succeeds
        /// </summary>
        [Fact]
        public async Task ReplayFixed_ReplaySucceeds_ReturnsRedirectToActionToIndex()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var payload = "{\"data\": \"test\"}";
            _adminApiClient.ReplayFixedMessageAsync(messageId, payload).Returns(true);

            // Act
            var result = await _controller.ReplayFixed(messageId, payload);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be(nameof(FailedMessagesController.Index));
        }

        /// <summary>
        ///     Returns NotFound result when replay with fixed payload fails
        /// </summary>
        [Fact]
        public async Task ReplayFixed_ReplayFails_ReturnsNotFoundResult()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var payload = "{\"data\": \"test\"}";
            _adminApiClient.ReplayFixedMessageAsync(messageId, payload).Returns(false);

            // Act
            var result = await _controller.ReplayFixed(messageId, payload);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        /// <summary>
        ///     Calls ReplayFixedMessageAsync with correct message ID and payload
        /// </summary>
        [Fact]
        public async Task ReplayFixed_WhenCalled_CallsReplayFixedMessageAsyncWithCorrectParameters()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var payload = "{\"data\": \"test\"}";
            _adminApiClient.ReplayFixedMessageAsync(messageId, payload).Returns(false);

            // Act
            await _controller.ReplayFixed(messageId, payload);

            // Assert
            await _adminApiClient.Received(1).ReplayFixedMessageAsync(messageId, payload);
        }

        #endregion
    }
}