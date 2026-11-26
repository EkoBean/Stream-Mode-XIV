using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace StreamModeXIV.Windows;

public sealed class SettingsWindow : Window
{
    private readonly Plugin plugin;
    private string extraAddons;

    public SettingsWindow(Plugin plugin) : base("Stream Mode XIV###StreamModeXIVSettings", ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.plugin = plugin;
        extraAddons = string.Join("\n", Plugin.Config.ExtraNameAddons);
    }

    public override void Draw()
    {
        var cfg = Plugin.Config;

        var enabled = cfg.StreamModeEnabled;
        if (ImGui.Checkbox("Stream Mode", ref enabled)) plugin.SetStreamMode(enabled);
        ImGui.SameLine();
        ImGui.TextDisabled(plugin.ObsRunning ? "(OBS running)" : "(OBS not detected)");

        ImGui.Separator();
        ImGui.TextUnformatted("On-screen toggle");

        var show = cfg.ShowToggle;
        if (ImGui.Checkbox("Show toggle on screen", ref show)) { cfg.ShowToggle = show; cfg.Save(); }

        var drag = cfg.ToggleDraggable;
        if (ImGui.Checkbox("Drag toggle with mouse", ref drag)) { cfg.ToggleDraggable = drag; cfg.Save(); }

        // Shown in pixels of the current game window; stored as a ratio so it follows resizes.
        var vp = ImGui.GetMainViewport();
        var pos = cfg.GetTogglePixel(vp.Pos, vp.Size) - vp.Pos;
        ImGui.SetNextItemWidth(200);
        if (ImGui.DragFloat2("Position (X, Y)", ref pos, 1f, 0f, MathF.Max(vp.Size.X, vp.Size.Y), "%.0f"))
            cfg.SetTogglePixel(new Vector2(MathF.Round(pos.X), MathF.Round(pos.Y)) + vp.Pos, vp.Pos, vp.Size);
        ImGui.SameLine();
        ImGui.TextDisabled($"({cfg.TogglePositionRatio.X:P0}, {cfg.TogglePositionRatio.Y:P0} of window)");

        ImGui.Separator();
        ImGui.TextUnformatted("OBS");
        var autoOn = cfg.AutoEnableOnObsStart;
        if (ImGui.Checkbox("Turn on Stream Mode when OBS starts", ref autoOn)) { cfg.AutoEnableOnObsStart = autoOn; cfg.Save(); }
        var autoOff = cfg.AutoDisableOnObsExit;
        if (ImGui.Checkbox("Turn off Stream Mode when OBS closes", ref autoOff)) { cfg.AutoDisableOnObsExit = autoOff; cfg.Save(); }

        ImGui.Separator();
        ImGui.TextUnformatted("Where to abbreviate");
        var chat = cfg.ReplaceInChat;
        if (ImGui.Checkbox("Chat log", ref chat)) { cfg.ReplaceInChat = chat; cfg.Save(); }

        if (ImGui.CollapsingHeader("Advanced: extra name-list windows"))
        {
            ImGui.TextWrapped("Addon names (one per line) where any \"Firstname Lastname\" should be abbreviated. Use /xldata -> Addon Inspector to find names.");
            if (ImGui.InputTextMultiline("##extra", ref extraAddons, 4000, new Vector2(300, 80)))
            {
                cfg.ExtraNameAddons = extraAddons.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                cfg.Save();
            }
            ImGui.TextDisabled("Reload the plugin to apply.");
        }
    }
}
