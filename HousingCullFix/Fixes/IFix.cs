using System;

namespace HousingCullFix.Fixes;

public interface IFix : IDisposable
{
    /// <summary>
    /// The name of this fix
    /// </summary>
    string Name { get; init; }
    
    /// <summary>
    /// The description of this fix
    /// </summary>
    string Description { get; init; }

    /// <summary>
    /// Whether this fix is enabled.
    /// </summary>
    bool Enabled { get; set; }
    
    /// <summary>
    /// Enable this fix
    /// </summary>
    void Enable();
    
    /// <summary>
    /// Disable this fix
    /// </summary>
    void Disable();
}
