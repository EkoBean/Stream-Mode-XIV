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

        // Position is stored relative to the game window, so it scales with window resizes.
        var vp = ImGui.GetMainViewport();
        var target = cfg.GetTogglePixel(vp.Pos, vp.Size);
        var dragging = cfg.ToggleDraggable && ImGui.IsMouseDown(ImGuiMouseButton.Left);
        ImGui.SetNextWindowPos(target, dragging ? ImGuiCond.Appearing : ImGuiCond.Always);
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

            if (dragging)
            {
                var pos = ImGui.GetWindowPos();
                if (Vector2.DistanceSquared(pos, target) > 0.25f)
                    cfg.SetTogglePixel(pos, vp.Pos, vp.Size);
            }
        }
        ImGui.End();
        ImGui.PopStyleVar();
    }
}
