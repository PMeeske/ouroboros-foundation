using FluentAssertions;
using Ouroboros.Core.Configuration;
using Xunit;

namespace Ouroboros.Tests.Configuration;

[Trait("Category", "Unit")]
public class QdrantSettingsTests
{
    [Fact]
    public void SectionPath_ShouldBeCorrect()
    {
        QdrantSettings.SectionPath.Should().Be("Ouroboros:Qdrant");
    }

    [Fact]
    public void Default_Host_ShouldBeLocalhost()
    {
        var settings = new QdrantSettings();
        settings.Host.Should().Be("localhost");
    }

    [Fact]
    public void Default_GrpcPort_ShouldBe6334()
    {
        var settings = new QdrantSettings();
        settings.GrpcPort.Should().Be(6334);
    }

    [Fact]
    public void Default_HttpPort_ShouldBe6333()
    {
        var settings = new QdrantSettings();
        settings.HttpPort.Should().Be(6333);
    }

    [Fact]
    public void Default_UseHttps_ShouldBeFalse()
    {
        var settings = new QdrantSettings();
        settings.UseHttps.Should().BeFalse();
    }

    [Fact]
    public void Default_ApiKey_ShouldBeNull()
    {
        var settings = new QdrantSettings();
        settings.ApiKey.Should().BeNull();
    }

    [Fact]
    public void Default_DefaultVectorSize_ShouldBe768()
    {
        var settings = new QdrantSettings();
        settings.DefaultVectorSize.Should().Be(768);
    }

    [Fact]
    public void GrpcEndpoint_WithHttps_ShouldUseHttpsScheme()
    {
        var settings = new QdrantSettings { UseHttps = true };
        settings.GrpcEndpoint.Should().Be("https://localhost:6334");
    }

    [Fact]
    public void GrpcEndpoint_WithoutHttps_ShouldUseHttpScheme()
    {
        var settings = new QdrantSettings();
        settings.GrpcEndpoint.Should().Be("http://localhost:6334");
    }

    [Fact]
    public void HttpEndpoint_WithCustomHost_ShouldReflect()
    {
        var settings = new QdrantSettings { Host = "qdrant.example.com", HttpPort = 443, UseHttps = true };
        settings.HttpEndpoint.Should().Be("https://qdrant.example.com:443");
    }

    [Fact]
    public void HttpEndpoint_Default_ShouldBeLocalhost6333()
    {
        var settings = new QdrantSettings();
        settings.HttpEndpoint.Should().Be("http://localhost:6333");
    }

    [Fact]
    public void Default_Cloud_ShouldBeNull()
    {
        var settings = new QdrantSettings();
        settings.Cloud.Should().BeNull();
    }

    [Fact]
    public void SetCloud_ShouldPersist()
    {
        var cloud = new QdrantCloudSettings { Endpoint = "https://abc.cloud.qdrant.io:6333", Enabled = true };
        var settings = new QdrantSettings { Cloud = cloud };
        settings.Cloud.Should().NotBeNull();
        settings.Cloud!.Enabled.Should().BeTrue();
        settings.Cloud.Endpoint.Should().Contain("cloud.qdrant.io");
    }
}

[Trait("Category", "Unit")]
public class QdrantCloudSettingsTests
{
    [Fact]
    public void Default_Endpoint_ShouldBeEmpty()
    {
        var settings = new QdrantCloudSettings();
        settings.Endpoint.Should().BeEmpty();
    }

    [Fact]
    public void Default_ApiKey_ShouldBeEmpty()
    {
        var settings = new QdrantCloudSettings();
        settings.ApiKey.Should().BeEmpty();
    }

    [Fact]
    public void Default_ClusterId_ShouldBeNull()
    {
        var settings = new QdrantCloudSettings();
        settings.ClusterId.Should().BeNull();
    }

    [Fact]
    public void Default_Enabled_ShouldBeFalse()
    {
        var settings = new QdrantCloudSettings();
        settings.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Default_EncryptionPrivateKey_ShouldBeNull()
    {
        var settings = new QdrantCloudSettings();
        settings.EncryptionPrivateKey.Should().BeNull();
    }
}
