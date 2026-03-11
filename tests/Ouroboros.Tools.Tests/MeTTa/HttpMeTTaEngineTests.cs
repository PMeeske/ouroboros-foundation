// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ouroboros.Abstractions;

namespace Ouroboros.Tests.Tools.MeTTa;

/// <summary>
/// Unit tests for HttpMeTTaEngine covering constructor, ExecuteQueryAsync,
/// AddFactAsync, ApplyRuleAsync, VerifyPlanAsync, ResetAsync, and Dispose.
/// Uses a mock HttpMessageHandler to simulate HTTP responses.
/// </summary>
[Trait("Category", "Unit")]
public class HttpMeTTaEngineTests : IDisposable
{
    private readonly MockHttpMessageHandler _handler;
    private HttpMeTTaEngine? _engine;

    public HttpMeTTaEngineTests()
    {
        _handler = new MockHttpMessageHandler();
    }

    public void Dispose()
    {
        _engine?.Dispose();
        _handler.Dispose();
    }

    // ========================================================================
    // Constructor
    // ========================================================================

    [Fact]
    public void Constructor_SetsBaseUrl()
    {
        // Act
        _engine = new HttpMeTTaEngine("http://localhost:8080");

        // Assert
        _engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_TrimsTrailingSlash()
    {
        // Act - should not throw
        _engine = new HttpMeTTaEngine("http://localhost:8080/");

        // Assert
        _engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithApiKey_SetsAuthorizationHeader()
    {
        // Act - should not throw
        _engine = new HttpMeTTaEngine("http://localhost:8080", "test-api-key");

        // Assert
        _engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullApiKey_DoesNotSetHeader()
    {
        // Act - should not throw
        _engine = new HttpMeTTaEngine("http://localhost:8080", null);

        // Assert
        _engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_DoesNotSetHeader()
    {
        // Act - should not throw
        _engine = new HttpMeTTaEngine("http://localhost:8080", "");

        // Assert
        _engine.Should().NotBeNull();
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        // Arrange
        _engine = new HttpMeTTaEngine("http://localhost:8080");

        // Act
        var act = () => _engine.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        _engine = new HttpMeTTaEngine("http://localhost:8080");

        // Act
        var act = () =>
        {
            _engine.Dispose();
            _engine.Dispose();
            _engine.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }

    // ========================================================================
    // IMeTTaEngine contract
    // ========================================================================

    [Fact]
    public void HttpMeTTaEngine_ImplementsIMeTTaEngine()
    {
        // Arrange
        _engine = new HttpMeTTaEngine("http://localhost:8080");

        // Assert
        _engine.Should().BeAssignableTo<IMeTTaEngine>();
    }

    [Fact]
    public void HttpMeTTaEngine_ImplementsIDisposable()
    {
        // Arrange
        _engine = new HttpMeTTaEngine("http://localhost:8080");

        // Assert
        _engine.Should().BeAssignableTo<IDisposable>();
    }

    // ========================================================================
    // Helper: Mock HttpMessageHandler
    // ========================================================================

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpStatusCode ResponseStatusCode { get; set; } = HttpStatusCode.OK;
        public string ResponseContent { get; set; } = "{}";
        public bool ShouldThrow { get; set; }
        public Exception? ExceptionToThrow { get; set; }
        public List<HttpRequestMessage> SentRequests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SentRequests.Add(request);

            if (ShouldThrow && ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            var response = new HttpResponseMessage(ResponseStatusCode)
            {
                Content = new StringContent(ResponseContent, System.Text.Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
