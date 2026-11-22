using System.Diagnostics;

namespace StreamModeXIV;

/// <summary>Polls for an OBS process and raises events when it starts or exits.</summary>
public sealed class ObsWatcher : IDisposable
{
    private static readonly string[] ProcessNames = ["obs64", "obs32", "obs"];
    private readonly Timer timer;
    private bool? lastRunning;

    public event Action? ObsStarted;
    public event Action? ObsExited;
    public bool IsRunning => lastRunning == true;

    public ObsWatcher()
    {
        timer = new Timer(_ => Poll(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
    }

    private void Poll()
    {
        bool running;
        try
        {
            running = ProcessNames.Any(n =>
            {
                var ps = Process.GetProcessesByName(n);
                foreach (var p in ps) p.Dispose();
                return ps.Length > 0;
            });
        }
        catch { return; }

        var previous = lastRunning;
        lastRunning = running;
        if (previous == running || (previous == null && !running)) return; // OBS already open at load counts as "started"

        // Hop onto the game thread before touching game/UI state.
        Plugin.Framework.RunOnFrameworkThread(() =>
        {
            if (running) ObsStarted?.Invoke();
            else ObsExited?.Invoke();
        });
    }

    public void Dispose() => timer.Dispose();
}
