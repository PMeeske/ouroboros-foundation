namespace Ouroboros.Core.Ethics;

/// <summary>
/// Clearance levels for ethical evaluation results.
/// </summary>
/// <remarks>
/// DESIGN NOTE: Both <see cref="RequiresHumanApproval"/> and <see cref="Denied"/> result in
/// <c>IsPermitted = false</c>. This is intentional - actions requiring approval cannot proceed
/// automatically and are treated as "not permitted" until human review occurs.
/// 
/// The distinction is preserved via the <c>Level</c> property to enable different handling:
/// - Denied: Absolute block, no path forward
/// - RequiresHumanApproval: Conditional block, can proceed with human authorization
/// 
/// TODO: Future enhancement - implement actual human-in-the-loop approval workflow
/// to handle RequiresHumanApproval actions differently from outright denials.
/// </remarks>
public enum EthicalClearanceLevel
{
    /// <summary>Action is permitted, no ethical concerns</summary>
    Permitted,

    /// <summary>Action is permitted with minor concerns to note</summary>
    PermittedWithConcerns,

    /// <summary>Action requires human approval before proceeding</summary>
    RequiresHumanApproval,

    /// <summary>Action is denied due to ethical violations</summary>
    Denied
}