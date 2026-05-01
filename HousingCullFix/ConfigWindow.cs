using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
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
        var value = configuration.ShadowLightMax;
        ImGui.Text($"Maximum Shadow Lights:");
        if (ImGui.SliderByte("###Maximum Shadow Lights", ref value, 0, 255))
        {
            configuration.ShadowLightMax = value;
        }

        if (ImGui.IsItemDeactivated())
        {
            configuration.Save();
            Plugin.SetShadowLights(value);
        }
    }
}
