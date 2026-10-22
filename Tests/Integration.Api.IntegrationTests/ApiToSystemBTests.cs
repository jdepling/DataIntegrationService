using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Integration.Api.IntegrationTests;

public sealed class ApiToSystemBTests(IntegrationApiFixture fixture): IClassFixture<IntegrationApiFixture>
{

    /// <summary>
    ///     Sends request through integration api and validates System B's database for the record.
    ///     This test validates this workflow: API -> Outbox -> Publisher -> Queue -> Worker -> SystemB API -> SystemB Db
    /// </summary>
    [Fact]
    public async Task PostMessage_ValidMessage_PersistsInSystemB()
    {
        var sourceId = $"E2E-{Guid.NewGuid():N}";
        const int customerId = 123456;
        const string name = "Automated Integration Test";
        const decimal amount = 125.50m;
        const string status = "Pending";

        var request = new
        {
            sourceId,
            data = new
            {
                customerId,
                name,
                amount,
                status
            }
        };

        using var response = await fixture.HttpClient.PostAsJsonAsync("api/messages", request);

        var responseBody = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(HttpStatusCode.Accepted,$"the API should accept the message; response body: {responseBody}");

        var savedMessage = await fixture.WaitForMessageAsync(sourceId);

        savedMessage.Should().NotBeNull();
        savedMessage!.CustomerId.Should().Be(customerId);
        savedMessage.Name.Should().Be(name);
        savedMessage.Amount.Should().Be(amount);
        savedMessage.Status.Should().Be(status);
        savedMessage.CreatedAt.Should().BeAfter(DateTime.MinValue);
    }

    /// <summary>
    ///     Verifies that posting a message without an authentication token returns an Unauthorized status code.
    /// </summary>
    [Fact]
    public async Task PostMessage_NoToken_ReturnsUnauthorized()
    {
        var request = new
        {
            sourceId = $"UNAUTH-{Guid.NewGuid():N}",
            data = new
            {
                customerId = 123456,
                name = "Unauthenticated Test",
                amount = 125.50m,
                status = "Pending"
            }
        };

        using var response = await fixture.UnauthenticatedHttpClient
            .PostAsJsonAsync("api/messages", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
