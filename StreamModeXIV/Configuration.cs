using System.Numerics;
using Dalamud.Configuration;

namespace StreamModeXIV;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    /// <summary>Stream Mode on/off. Shared by the on-screen toggle and the settings checkbox.</summary>
    public bool StreamModeEnabled { get; set; }

    /// <summary>Show the "Stream Mode (O  )" toggle on screen.</summary>
    public bool ShowToggle { get; set; } = true;

    /// <summary>Top-left screen position of the toggle, in pixels.</summary>
    public Vector2 TogglePosition { get; set; } = new(100, 100);

    /// <summary>Let the user drag the toggle with the mouse.</summary>
    public bool ToggleDraggable { get; set; }

    public bool AutoEnableOnObsStart { get; set; } = true;
    public bool AutoDisableOnObsExit { get; set; } = true;

    /// <summary>Also abbreviate names in the chat log.</summary>
    public bool ReplaceInChat { get; set; }

    /// <summary>Extra addon names (one per line) to treat as "name lists" where any Firstname Lastname gets abbreviated.</summary>
    public List<string> ExtraNameAddons { get; set; } = [];

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
