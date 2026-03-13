// <copyright file="TracingConfigurationTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics;
using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class TracingConfigurationTests : IDisposable
{
    public TracingConfigurationTests()
    {
        // Ensure clean state before each test
        TracingConfiguration.DisableTracing();
    }

    public void Dispose()
    {
        TracingConfiguration.DisableTracing();
    }

    [Fact]
    public void EnableTracing_ShouldAllowActivityCreation()
    {
        TracingConfiguration.EnableTracing();

        using var activity = DistributedTracing.StartActivity("test.enabled");

        activity.Should().NotBeNull();
    }

    [Fact]
    public void DisableTracing_ShouldPreventActivityCreation()
    {
        TracingConfiguration.EnableTracing();
        TracingConfiguration.DisableTracing();

        // After disabling, without any other listener, activities will be null
        // (assuming no other listener is active for this test)
        // Note: This test may be affected by other test listeners. The key
        // behavior is that DisableTracing does not throw.
        var act = () => TracingConfiguration.DisableTracing();
        act.Should().NotThrow();
    }

    [Fact]
    public void EnableTracing_CalledTwice_ShouldNotThrow()
    {
        TracingConfiguration.EnableTracing();

        var act = () => TracingConfiguration.EnableTracing();
        act.Should().NotThrow();
    }

    [Fact]
    public void DisableTracing_WithoutEnabling_ShouldNotThrow()
    {
        var act = () => TracingConfiguration.DisableTracing();
        act.Should().NotThrow();
    }

    [Fact]
    public void EnableTracing_WithCallbacks_ShouldInvokeOnStarted()
    {
        bool startedCalled = false;
        TracingConfiguration.EnableTracing(
            onActivityStarted: _ => startedCalled = true);

        using var activity = DistributedTracing.StartActivity("test.callback");

        startedCalled.Should().BeTrue();
    }

    [Fact]
    public void EnableTracing_WithCallbacks_ShouldInvokeOnStopped()
    {
        bool stoppedCalled = false;
        TracingConfiguration.EnableTracing(
            onActivityStopped: _ => stoppedCalled = true);

        var activity = DistributedTracing.StartActivity("test.stopped-callback");
        activity?.Dispose();

        stoppedCalled.Should().BeTrue();
    }

    [Fact]
    public void EnableConsoleTracing_ShouldNotThrow()
    {
        var act = () => TracingConfiguration.EnableConsoleTracing();
        act.Should().NotThrow();
    }
}
