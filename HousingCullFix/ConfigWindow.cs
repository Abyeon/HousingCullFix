using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace HousingCullFix;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    
    public ConfigWindow(Plugin plugin) : base("Housing Cull Fix")
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize;

        configuration = plugin.Configuration;
    }
    
    public void Dispose() { }

    public override void Draw()
    {
        var value = configuration.EnableCastShadows;
        if (ImGui.Checkbox("Enable Cast Shadows", ref value))
        {
            configuration.EnableCastShadows = value;
        }

        if (ImGui.IsItemDeactivated())
        {
            configuration.Save();
            Plugin.SetShadowLights(value);
        }
        
        ImGuiComponents.HelpMarker("This toggles Cast Shadows within housing.\nThe in-game setting can be found at Graphics Settings > Cast Shadows");
    }
}
