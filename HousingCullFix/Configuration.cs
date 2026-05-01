using System;
using Dalamud.Configuration;

namespace HousingCullFix;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public byte ShadowLightMax { get; set; } = 14;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
