using Microsoft.Extensions.DependencyInjection;
using Ouroboros.Abstractions.Agent.Dispatch;
using Ouroboros.Core.Dispatch;

namespace Ouroboros.Core.Tests.Dispatch;

[Trait("Category", "Unit")]
public sealed class DispatchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOuroborosDispatch_RegistersDispatcher()
    {
        var services = new ServiceCollection();
        services.AddOuroborosDispatch(typeof(DispatchServiceCollectionExtensionsTests).Assembly);

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetService<IDispatcher>();

        dispatcher.Should().NotBeNull();
        dispatcher.Should().BeOfType<ServiceProviderDispatcher>();
    }

    [Fact]
    public void AddOuroborosDispatch_DoesNotReplaceExistingDispatcher()
    {
        var mockDispatcher = new Mock<IDispatcher>();
        var services = new ServiceCollection();
        services.AddSingleton(mockDispatcher.Object);
        services.AddOuroborosDispatch(typeof(DispatchServiceCollectionExtensionsTests).Assembly);

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetService<IDispatcher>();

        dispatcher.Should().BeSameAs(mockDispatcher.Object);
    }

    [Fact]
    public void AddOuroborosDispatch_ScansAssembly_RegistersHandlers()
    {
        var services = new ServiceCollection();
        services.AddOuroborosDispatch(typeof(DispatchServiceCollectionExtensionsTests).Assembly);

        var provider = services.BuildServiceProvider();

        // TestCommandHandler and TestQueryHandler are defined in ServiceProviderDispatcherTests.cs
        // and are in the same assembly, so they should be auto-discovered.
        var commandHandler = provider.GetService<ICommandHandler<TestCommand, string>>();
        var queryHandler = provider.GetService<IQueryHandler<TestQuery, int>>();

        commandHandler.Should().NotBeNull();
        commandHandler.Should().BeOfType<TestCommandHandler>();

        queryHandler.Should().NotBeNull();
        queryHandler.Should().BeOfType<TestQueryHandler>();
    }

    [Fact]
    public void AddOuroborosDispatch_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddOuroborosDispatch(typeof(DispatchServiceCollectionExtensionsTests).Assembly);

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddOuroborosDispatch_CalledTwice_DoesNotDuplicate()
    {
        var services = new ServiceCollection();
        services.AddOuroborosDispatch(typeof(DispatchServiceCollectionExtensionsTests).Assembly);
        services.AddOuroborosDispatch(typeof(DispatchServiceCollectionExtensionsTests).Assembly);

        // TryAdd prevents duplicates
        var dispatcherRegistrations = services.Where(
            sd => sd.ServiceType == typeof(IDispatcher)).ToList();
        dispatcherRegistrations.Should().HaveCount(1);
    }
}
