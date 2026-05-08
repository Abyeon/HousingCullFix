using System;
using System.Collections.Generic;
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
        
        //DrawDebug();
    }

    public void DrawFixDropdown()
    {
        var name = configuration.SelectedFix;

        bool fixSelected = plugin.FixIndex == -1;
        var preview = fixSelected ? "None" : plugin.Fixes[plugin.FixIndex].Name;
        
        using var popup = ImRaii.Combo("Culling Fix To Use", preview);
        if (!popup.Success) return;

        uint id = 0;
        ImGui.PushID(++id);
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

    private unsafe void CopyableAddr(IntPtr ptr)
    {
        var addr = $"{ptr:X}";
        if (ImGui.Selectable(addr))
        {
            ImGui.SetClipboardText(addr);
        }
    }

    public unsafe void DrawDebug()
    {
        if (ImGui.Button("Redraw"))
        {
            Scene.RedrawObjects();
        }
        
        var graphics = GraphicsConfig.Instance();
        ImGui.Checkbox("IsIndoor", ref graphics->IsIndoor);

        var man = HousingManager.Instance();
        if (man->IsInside())
        {
            var lighting = (uint)Math.Floor(man->IndoorTerritory->BrightnessTarget * 5);
            if (ImGui.SliderUInt("Brightness", ref lighting, 0, 5))
            {
                plugin.HouseFunctions.SetInteriorLight(lighting);
            }
        }

        var ptr = AreaCullingManager.Instance();
        CopyableAddr((IntPtr)ptr);
        
        try
        {
            var arrayPtr = &ptr->CullObjects;
            CopyableAddr((IntPtr)arrayPtr);
            
            uint id = 0;
            foreach (ref var obj in ptr->CullObjects)
            {
                ImGui.PushID(id++);
                
                var pos = (Vector3)obj.Position;
                ImGui.InputFloat3("Position", ref pos);
                obj.Position = pos;
                
                var off = (Vector3)obj.Offset;
                ImGui.InputFloat3("Offset", ref off);
                obj.Offset = off;
                
                ImGui.InputInt("Unk0", ref obj.Unk0);
                ImGui.InputFloat("Distance", ref obj.Distance);
                ImGui.Spacing();
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e.ToString());
        }
        
        return;
        //
        // var fields = cullMan.GetType().GetFields();
        // foreach (var field in fields)
        // {
        //     var type = field.FieldType.Name;
        //     var name = field.Name;
        //     var value = field.GetValue(cullMan);
        //     
        //     ImGui.TextColored(ImGuiColors.ParsedBlue, type);
        //     
        //     ImGui.SameLine();
        //     ImGui.Text(name);
        //     ImGui.SameLine();
        //     ImGui.TextColored(ImGuiColors.DalamudViolet,$"{value:X}");
        //     if (ImGui.IsItemClicked())
        //     {
        //         ImGui.SetClipboardText($"{value:X}");
        //     }
        // }
    }
}
