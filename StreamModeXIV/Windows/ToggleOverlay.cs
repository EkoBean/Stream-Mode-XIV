using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace StreamModeXIV.Windows;

/// <summary>
/// The bare on-screen toggle: "Stream Mode (O  )". No window chrome, no background.
/// Position comes only from the plugin config (NoSavedSettings => nothing in imgui.ini).
/// </summary>
public sealed class ToggleOverlay(Plugin plugin)
{
    private const float TrackW = 34f, TrackH = 18f;

    public void Draw()
    {
        var cfg = Plugin.Config;
        if (!cfg.ShowToggle) return;

        var flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground |
                    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav |
                    ImGuiWindowFlags.NoDocking;
        if (!cfg.ToggleDraggable) flags |= ImGuiWindowFlags.NoMove;

        // While dragging, let ImGui own the position; otherwise force it from config.
        ImGui.SetNextWindowPos(cfg.TogglePosition, cfg.ToggleDraggable ? ImGuiCond.Appearing : ImGuiCond.Always);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 2));
        if (ImGui.Begin("##StreamModeXIVToggle", flags))
        {
            var on = cfg.StreamModeEnabled;
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Stream Mode");
            ImGui.SameLine();

            var p = ImGui.GetCursorScreenPos();
            p.Y += (ImGui.GetFrameHeight() - TrackH) / 2;
            ImGui.SetCursorScreenPos(p);
            if (ImGui.InvisibleButton("##sm", new Vector2(TrackW, TrackH)))
                plugin.SetStreamMode(!on);

            var dl = ImGui.GetWindowDrawList();
            var r = TrackH / 2;
            var track = on ? new Vector4(0.25f, 0.75f, 0.35f, 1f) : new Vector4(0.35f, 0.35f, 0.35f, 0.9f);
            dl.AddRectFilled(p, p + new Vector2(TrackW, TrackH), ImGui.GetColorU32(track), r);
            var knobX = on ? p.X + TrackW - r : p.X + r;
            dl.AddCircleFilled(new Vector2(knobX, p.Y + r), r - 2, ImGui.GetColorU32(new Vector4(1, 1, 1, 1)));

            if (cfg.ToggleDraggable && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                var pos = ImGui.GetWindowPos();
                if (Vector2.DistanceSquared(pos, cfg.TogglePosition) > 0.25f)
                {
                    cfg.TogglePosition = pos;
                    cfg.Save();
                }
            }
        }
        ImGui.End();
        ImGui.PopStyleVar();
    }
}
