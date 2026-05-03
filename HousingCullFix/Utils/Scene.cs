using System;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Serilog;

namespace HousingCullFix;

public static unsafe class Scene
{
    public static void SetCastShadows(bool enabled)
    {
        var config = GraphicsConfig.Instance();
        if (config == null) throw new NullReferenceException("GraphicsConfig.Instance() returned null");
        
        var man = HousingManager.Instance();
        if (man == null) return;

        if (!enabled && man->IsInside())
        {
            config->ShadowLightValidType = 0;
        }
        else
        {
            if (Plugin.GameConfig.System.TryGetUInt("ShadowLightValidType", out var maxShadows))
            {
                config->ShadowLightValidType = maxShadows switch
                {
                    0 => 8,
                    1 => 14,
                    2 => 20,
                    _ => throw new ArgumentOutOfRangeException($"ShadowLightValidType returned an unexpected value {maxShadows}")
                };
            }
            else
            {
                Log.Error("Could not find ShadowLightValidType.");
            }
        }
    }
    
    public static void RedrawObjects()
    {
        var man = HousingManager.Instance();
        if (man == null || !man->IsInside()) return;

        var furnitureMan = man->GetFurnitureManager();
        
        foreach (ref var ptr in furnitureMan->ObjectManager.ObjectArray.Objects)
        {
            var gameObject = ptr.Value;
            if (gameObject == null) continue;
            
            gameObject->DisableDraw();
            
            Plugin.Log.Verbose($"Redrawing {gameObject->NameString} : {((IntPtr)gameObject):X}");
        }
    }
}
