using Microsoft.Extensions.DependencyInjection;
using Ouroboros.Abstractions.Agent.Dispatch;
using Ouroboros.Core.Dispatch;

namespace Ouroboros.Core.Tests.Dispatch;

// Test command/query types
public record TestCommand(string Value) : ICommand<string>;
public record TestQuery(int Id) : IQuery<int>;

public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<string> HandleAsync(TestCommand command, CancellationToken ct = default)
        => Task.FromResult($"handled:{command.Value}");
}

public class TestQueryHandler : IQueryHandler<TestQuery, int>
{
    public Task<int> HandleAsync(TestQuery query, CancellationToken ct = default)
        => Task.FromResult(query.Id * 10);
}

[Trait("Category", "Unit")]
public sealed class ServiceProviderDispatcherTests
{
    [Fact]
    public void Constructor_NullServiceProvider_Throws()
    {
        var act = () => new ServiceProviderDispatcher(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendAsync_ResolvesAndInvokesCommandHandler()
    {
        var services = new ServiceCollection();
        services.AddTransient<ICommandHandler<TestCommand, string>, TestCommandHandler>();
        var provider = services.BuildServiceProvider();

        var dispatcher = new ServiceProviderDispatcher(provider);
        var result = await dispatcher.SendAsync<string>(new TestCommand("test")).ConfigureAwait(false);

        result.Should().Be("handled:test");
    }

    [Fact]
    public async Task SendAsync_NoHandler_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var dispatcher = new ServiceProviderDispatcher(provider);

        var act = () => dispatcher.SendAsync<string>(new TestCommand("test"));
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No command handler*TestCommand*").ConfigureAwait(false);
    }

    [Fact]
    public async Task QueryAsync_ResolvesAndInvokesQueryHandler()
    {
        var services = new ServiceCollection();
        services.AddTransient<IQueryHandler<TestQuery, int>, TestQueryHandler>();
        var provider = services.BuildServiceProvider();

        var dispatcher = new ServiceProviderDispatcher(provider);
        var result = await dispatcher.QueryAsync<int>(new TestQuery(5)).ConfigureAwait(false);

        result.Should().Be(50);
    }

    [Fact]
    public async Task QueryAsync_NoHandler_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var dispatcher = new ServiceProviderDispatcher(provider);

        var act = () => dispatcher.QueryAsync<int>(new TestQuery(5));
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No query handler*TestQuery*").ConfigureAwait(false);
    }

    [Fact]
    public async Task SendAsync_PassesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;

        var handler = new Mock<ICommandHandler<TestCommand, string>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .Callback<TestCommand, CancellationToken>((_, ct) => capturedToken = ct)
            .ReturnsAsync("ok");

        var services = new ServiceCollection();
        services.AddSingleton(handler.Object);
        // Register as the interface type
        services.AddSingleton<ICommandHandler<TestCommand, string>>(handler.Object);
        var provider = services.BuildServiceProvider();

        var dispatcher = new ServiceProviderDispatcher(provider);
        await dispatcher.SendAsync<string>(new TestCommand("test"), cts.Token).ConfigureAwait(false);

        capturedToken.Should().Be(cts.Token);
    }
}
