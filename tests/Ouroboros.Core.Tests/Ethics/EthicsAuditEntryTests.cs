using Ouroboros.Core.Ethics;
using Moq;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
public class EthicsAuditEntryTests
{
    [Fact]
    public void EthicsAuditEntry_ShouldBeCreatable()
    {
        // Verify EthicsAuditEntry type exists and is accessible
        typeof(EthicsAuditEntry).Should().NotBeNull();
    }

    [Fact]
    public void EthicsAuditEntry_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(EthicsAuditEntry).GetProperty("Id").Should().NotBeNull();
        typeof(EthicsAuditEntry).GetProperty("Timestamp").Should().NotBeNull();
        typeof(EthicsAuditEntry).GetProperty("AgentId").Should().NotBeNull();
        typeof(EthicsAuditEntry).GetProperty("UserId").Should().NotBeNull();
        typeof(EthicsAuditEntry).GetProperty("EvaluationType").Should().NotBeNull();
    }
}
