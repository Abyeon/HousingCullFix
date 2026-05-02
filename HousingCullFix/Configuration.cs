using System;
using Dalamud.Configuration;
using HousingCullFix.Fixes;

namespace HousingCullFix;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool EnableCastShadows { get; set; } = true;
    public string SelectedFix { get; set; } = "FakeOutside";

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
