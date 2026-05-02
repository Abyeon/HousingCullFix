using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using HousingCullFix.Fixes;

namespace HousingCullFix;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly Plugin plugin;
    
    public ConfigWindow(Plugin plugin) : base("Housing Cull Fix")
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize;

        configuration = plugin.Configuration;
        this.plugin = plugin;
    }
    
    public void Dispose() { }

    public override void Draw()
    {
        DrawFixDropdown();
        
        var castShadows = configuration.EnableCastShadows;
        if (ImGui.Checkbox("Enable Cast Shadows", ref castShadows))
        {
            configuration.EnableCastShadows = castShadows;
        }

        if (ImGui.IsItemDeactivated())
        {
            configuration.Save();
            Plugin.SetCastShadows(castShadows);
        }
        
        ImGuiComponents.HelpMarker("This toggles Cast Shadows within housing.\nThe in-game setting can be found at Graphics Settings > Cast Shadows");
    }

    public void DrawFixDropdown()
    {
        var name = configuration.SelectedFix;
        
        using var popup = ImRaii.Combo("Culling Fix To Use", plugin.Fixes[plugin.FixIndex].Name);
        if (!popup.Success) return;

        uint id = 0;
        foreach (var fix in plugin.Fixes)
        {
            ImGui.PushID(++id);
            var assemblyName = fix.GetType().Name;

            if (ImGui.Selectable(fix.Name + $"###{id}", name == assemblyName))
            {
                plugin.SetFix(assemblyName);
                configuration.SelectedFix = assemblyName;
                configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(fix.Description);
            }
        }
    }
}
