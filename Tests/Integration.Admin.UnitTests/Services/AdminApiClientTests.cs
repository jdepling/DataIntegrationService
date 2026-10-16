using System.Net;
using FluentAssertions;
using Integration.Admin.Models;
using Integration.Admin.Services;
using Integration.Data;
using NSubstitute;

namespace Integration.Admin.UnitTests.Services
{
    public class AdminApiClientTests
    {
        private readonly IHttpClientAdapter _mockHttpClient;
        private readonly AdminApiClient _adminApiClient;

        public AdminApiClientTests()
        {
            _mockHttpClient = Substitute.For<IHttpClientAdapter>();
            _adminApiClient = new AdminApiClient(_mockHttpClient);
        }

        /// <summary>
        ///     Tests that ReplayMessageAsync returns true when the API responds with a success status code.
        ///     This verifies the happy path where a message is successfully replayed.
        /// </summary>
        [Fact]
        public async Task ReplayMessageAsync_ReplaySucceeds_ReturnsTrue()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHttpClient.PostAsync($"api/admin/messages/{messageId}/replay", null)
                .Returns(x => Task.FromResult(response));

            // Act
            var result = await _adminApiClient.ReplayMessageAsync(messageId);

            // Assert
            result.Should().BeTrue();
            await _mockHttpClient.Received(1).PostAsync($"api/admin/messages/{messageId}/replay", null);
        }

        /// <summary>
        ///     Tests that ReplayMessageAsync returns false when the API responds with a 400 Bad Request status code.
        ///     This verifies error handling for client-side request errors.
        /// </summary>
        [Fact]
        public async Task ReplayMessageAsync_ReplayFails_ReturnsFalse()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _mockHttpClient.PostAsync($"api/admin/messages/{messageId}/replay", null)
                .Returns(x => Task.FromResult(response));

            // Act
            var result = await _adminApiClient.ReplayMessageAsync(messageId);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        ///     Tests that ReplayMessageAsync returns false when the API responds with a 500 Internal Server Error status code.
        ///     This verifies error handling for server-side errors.
        /// </summary>
        [Fact]
        public async Task ReplayMessageAsync_ServerError_ReturnsFalse()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _mockHttpClient.PostAsync($"api/admin/messages/{messageId}/replay", null)
                .Returns(x => Task.FromResult(response));

            // Act
            var result = await _adminApiClient.ReplayMessageAsync(messageId);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        ///     Tests that ReplayMessageAsync returns false when the API responds with a 401 Unauthorized status code.
        ///     This verifies error handling for authentication failures.
        /// </summary>
        [Fact]
        public async Task ReplayMessageAsync_Unauthorized_ReturnsFalse()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            _mockHttpClient.PostAsync($"api/admin/messages/{messageId}/replay", null)
                .Returns(x => Task.FromResult(response));

            // Act
            var result = await _adminApiClient.ReplayMessageAsync(messageId);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        ///     Tests that ReplayMessageAsync returns false when the API responds with a 404 Not Found status code.
        ///     This verifies error handling when the requested message does not exist.
        /// </summary>
        [Fact]
        public async Task ReplayMessageAsync_NotFound_ReturnsFalse()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            _mockHttpClient.PostAsync($"api/admin/messages/{messageId}/replay", null)
                .Returns(x => Task.FromResult(response));

            // Act
            var result = await _adminApiClient.ReplayMessageAsync(messageId);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        ///     Tests that ReplayFixedMessageAsync returns true when the API responds with a success status code.
        ///     This verifies that a message can be replayed with a corrected payload.
        /// </summary>
        [Fact]
        public async Task ReplayFixedMessageAsync_Succeeds_ReturnsTrue()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var payload = "test payload";
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHttpClient.PostAsJsonAsync($"api/admin/messages/{messageId}/replay-fixed", payload)
                .Returns(x => Task.FromResult(response));

            // Act
            var result = await _adminApiClient.ReplayFixedMessageAsync(messageId, payload);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        ///     Tests that ReplayFixedMessageAsync returns false when the API responds with a 400 Bad Request status code.
        ///     This verifies error handling when replaying a message with a fixed payload fails.
        /// </summary>
        [Fact]
        public async Task ReplayFixedMessageAsync_Fails_ReturnsFalse()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var payload = "test payload";
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _mockHttpClient.PostAsJsonAsync($"api/admin/messages/{messageId}/replay-fixed", payload)
                .Returns(x => Task.FromResult(response));

            // Act
            var result = await _adminApiClient.ReplayFixedMessageAsync(messageId, payload);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        ///     Tests that GetFailedMessagesAsync returns a list of failed messages from the API.
        ///     This verifies the retrieval and deserialization of failed message data.
        /// </summary>
        [Fact]
        public async Task GetFailedMessagesAsync_ReturnsMessages()
        {
            // Arrange
            var messages = new List<FailedMessageViewModel>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    SourceId = "source-1",
                    Payload = "test",
                    ErrorMessage = "Error",
                    FailureType = FailureType.Unknown,
                    CreatedAt = DateTime.UtcNow,
                    LastAttemptAt = DateTime.UtcNow
                }
            };

            _mockHttpClient.GetFromJsonAsync<List<FailedMessageViewModel>>("api/admin/messages")
                .Returns(x => Task.FromResult<List<FailedMessageViewModel>?>(messages));

            // Act
            var result = await _adminApiClient.GetFailedMessagesAsync();

            // Assert
            result.Should().BeEquivalentTo(messages);
        }

        /// <summary>
        ///     Tests that GetFailedMessagesAsync returns an empty list when the API returns null.
        ///     This verifies the default behavior when no failed messages are available.
        /// </summary>
        [Fact]
        public async Task GetFailedMessagesAsync_ReturnsEmptyList_WhenNoneExist()
        {
            // Arrange
            _mockHttpClient.GetFromJsonAsync<List<FailedMessageViewModel>>("api/admin/messages")
                .Returns(x => Task.FromResult<List<FailedMessageViewModel>?>(null));

            // Act
            var result = await _adminApiClient.GetFailedMessagesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        ///     Tests that GetFailedMessageAsync returns a single failed message from the API.
        ///     This verifies the retrieval and deserialization of a specific failed message by ID.
        /// </summary>
        [Fact]
        public async Task GetFailedMessageAsync_ReturnsMessage()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var message = new FailedMessageViewModel 
            { 
                Id = messageId, 
                SourceId = "source-1",
                Payload = "test",
                ErrorMessage = "Error",
                FailureType = FailureType.Unknown,
                CreatedAt = DateTime.UtcNow,
                LastAttemptAt = DateTime.UtcNow
            };

            _mockHttpClient.GetFromJsonAsync<FailedMessageViewModel>($"api/admin/messages/{messageId}", Arg.Any<CancellationToken>())
                .Returns(x => Task.FromResult<FailedMessageViewModel?>(message));

            // Act
            var result = await _adminApiClient.GetFailedMessageAsync(messageId);

            // Assert
            result.Should().BeEquivalentTo(message);
        }

        /// <summary>
        ///     Tests that GetFailedMessageAsync returns null when the API cannot find the requested message.
        ///     This verifies the behavior when a message with the specified ID does not exist.
        /// </summary>
        [Fact]
        public async Task GetFailedMessageAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var messageId = Guid.NewGuid();

            _mockHttpClient.GetFromJsonAsync<FailedMessageViewModel>($"api/admin/messages/{messageId}", Arg.Any<CancellationToken>())
                .Returns(x => Task.FromResult<FailedMessageViewModel?>(null));

            // Act
            var result = await _adminApiClient.GetFailedMessageAsync(messageId);

            // Assert
            result.Should().BeNull();
        }
    }
}
