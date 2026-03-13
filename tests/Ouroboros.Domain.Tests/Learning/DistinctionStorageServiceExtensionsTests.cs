using Microsoft.Extensions.DependencyInjection;
using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Domain.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public class DistinctionStorageServiceExtensionsTests
{
    [Fact]
    public void AddDistinctionStorage_ShouldRegisterServices()
    {
        var services = new ServiceCollection();

        services.AddDistinctionStorage();

        services.Should().Contain(sd => sd.ServiceType == typeof(DistinctionStorageConfig));
        services.Should().Contain(sd => sd.ServiceType == typeof(IDistinctionWeightStorage));
        services.Should().Contain(sd => sd.ServiceType == typeof(QdrantDistinctionMetadataStorage));
    }

    [Fact]
    public void AddDistinctionStorage_WithCustomConfig_ShouldUseCustomConfig()
    {
        var services = new ServiceCollection();
        var customConfig = new DistinctionStorageConfig("/custom/path", 1024L * 1024 * 1024, TimeSpan.FromDays(30));

        services.AddDistinctionStorage(customConfig);

        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<DistinctionStorageConfig>();
        config.StoragePath.Should().Be("/custom/path");
    }
}
