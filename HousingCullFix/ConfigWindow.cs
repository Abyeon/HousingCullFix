using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using HousingCullFix.Fixes;
using HousingCullFix.Structs;
using HousingCullFix.Utils;

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
            Scene.SetCastShadows(castShadows);
        }
        
        ImGuiComponents.HelpMarker("This toggles Cast Shadows within housing.\n" +
                                   "The in-game setting can be found at Graphics Settings > Cast Shadows\n" +
                                   "Disabling this may improve performance significantly in certain houses.");
    }

    public void DrawFixDropdown()
    {
        var name = configuration.SelectedFix;

        bool fixSelected = plugin.FixIndex == -1;
        var preview = fixSelected ? "None" : plugin.Fixes[plugin.FixIndex].Name;
        
        using var popup = ImRaii.Combo("Culling Fix To Use", preview);
        if (!popup.Success) return;

        var id = 0;
        using var pushedID = ImRaii.PushId(id++);
        if (ImGui.Selectable("None", fixSelected))
        {
            plugin.SetFix("");
            configuration.SelectedFix = "";
            configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Brings back vanilla behaviour.");
        }
        
        foreach (var fix in plugin.Fixes)
        {
            using var _ = ImRaii.PushId(id++);
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
