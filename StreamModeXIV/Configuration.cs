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

    /// <summary>Legacy absolute pixel position (pre 0.1.1). Only read once to migrate.</summary>
    public Vector2 TogglePosition { get; set; } = new(100, 100);

    /// <summary>
    /// Toggle position relative to the game window: 0..1 of its width/height.
    /// Negative = not set yet (migrated from <see cref="TogglePosition"/> on first draw).
    /// </summary>
    public Vector2 TogglePositionRatio { get; set; } = new(-1, -1);

    /// <summary>Toggle position in screen pixels for the given game window.</summary>
    public Vector2 GetTogglePixel(Vector2 windowPos, Vector2 windowSize)
    {
        if (TogglePositionRatio.X < 0 || TogglePositionRatio.Y < 0)
            SetTogglePixel(TogglePosition + windowPos, windowPos, windowSize);
        return windowPos + TogglePositionRatio * windowSize;
    }

    public void SetTogglePixel(Vector2 pixel, Vector2 windowPos, Vector2 windowSize)
    {
        if (windowSize.X <= 0 || windowSize.Y <= 0) return;
        var r = (pixel - windowPos) / windowSize;
        TogglePositionRatio = Vector2.Clamp(r, Vector2.Zero, Vector2.One);
        Save();
    }

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
