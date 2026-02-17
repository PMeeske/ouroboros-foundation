namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Affordance type classification based on ecological psychology.
/// </summary>
public enum AffordanceType
{
    /// <summary>Surface can be traversed (walked on, driven on).</summary>
    Traversable,

    /// <summary>Object can be grasped and manipulated.</summary>
    Graspable,

    /// <summary>Object can be pushed or moved.</summary>
    Pushable,

    /// <summary>Object can be pulled toward agent.</summary>
    Pullable,

    /// <summary>Object can be lifted.</summary>
    Liftable,

    /// <summary>Object can be rotated.</summary>
    Rotatable,

    /// <summary>Space can be entered or occupied.</summary>
    Enterable,

    /// <summary>Object can be climbed.</summary>
    Climbable,

    /// <summary>Object is sittable (chair, bench).</summary>
    Sittable,

    /// <summary>Container can hold objects.</summary>
    Containable,

    /// <summary>Surface provides support for placing objects.</summary>
    Supportive,

    /// <summary>Object can be broken or destroyed.</summary>
    Breakable,

    /// <summary>Object can be combined with others.</summary>
    Combinable,

    /// <summary>Object can be activated (button, lever).</summary>
    Activatable,

    /// <summary>Object blocks movement or view.</summary>
    Obstructive,

    /// <summary>Surface is slippery.</summary>
    Slippery,

    /// <summary>Surface is dangerous.</summary>
    Hazardous,

    /// <summary>Custom affordance type.</summary>
    Custom,
}