using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using StreamModeXIV.Replacers;
using StreamModeXIV.Windows;

namespace StreamModeXIV;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static IPartyList PartyList { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static INamePlateGui NamePlateGui { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;

    private const string Command = "/streammode";

    public static Configuration Config { get; private set; } = null!;

    private readonly WindowSystem windows = new("StreamModeXIV");
    private readonly SettingsWindow settingsWindow;
    private readonly ToggleOverlay toggle;
    private readonly ObsWatcher obs;
    private readonly NamePlateReplacer namePlates;
    private readonly ChatReplacer chat;
    private readonly AddonTextReplacer addons;
    private DateTime nextScan = DateTime.MinValue;

    public Plugin()
    {
        Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        GameTextLoader.LoadAsync();
        namePlates = new NamePlateReplacer();
        chat = new ChatReplacer();
        addons = new AddonTextReplacer();

        settingsWindow = new SettingsWindow(this);
        windows.AddWindow(settingsWindow);
        toggle = new ToggleOverlay(this);

        obs = new ObsWatcher();
        obs.ObsStarted += OnObsStarted;
        obs.ObsExited += OnObsExited;

        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleSettings;
        PluginInterface.UiBuilder.OpenMainUi += ToggleSettings;
        Framework.Update += OnUpdate;

        CommandManager.AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open Stream Mode XIV settings. \"/streammode on|off|toggle\" switches Stream Mode.",
        });
    }

    public bool ObsRunning => obs.IsRunning;

    /// <summary>Single entry point for switching Stream Mode (toggle, settings, command, OBS).</summary>
    public void SetStreamMode(bool enabled)
    {
        if (Config.StreamModeEnabled == enabled) return;
        Config.StreamModeEnabled = enabled;
        Config.Save();
        if (enabled) LearnNames();
        else addons.RestoreAll();
        NamePlateGui.RequestRedraw();
    }

    public void ToggleSettings() => settingsWindow.Toggle();

    private void OnObsStarted()
    {
        if (!Config.AutoEnableOnObsStart) return;
        // If the toggle is already on screen the user can flip it there; otherwise open settings.
        if (!Config.ShowToggle) settingsWindow.IsOpen = true;
        SetStreamMode(true);
    }

    private void OnObsExited()
    {
        if (Config.AutoDisableOnObsExit) SetStreamMode(false);
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "on": SetStreamMode(true); break;
            case "off": SetStreamMode(false); break;
            case "toggle": SetStreamMode(!Config.StreamModeEnabled); break;
            default: ToggleSettings(); break;
        }
    }

    private void OnUpdate(IFramework framework)
    {
        if (!Config.StreamModeEnabled || DateTime.UtcNow < nextScan) return;
        nextScan = DateTime.UtcNow.AddSeconds(1);
        LearnNames();
    }

    /// <summary>Collect names of every player character currently known to the client.</summary>
    private static void LearnNames()
    {
        foreach (var obj in ObjectTable)
            if (obj is IPlayerCharacter pc)
                NameAbbreviator.Learn(pc.Name.TextValue);
        foreach (var member in PartyList)
            NameAbbreviator.Learn(member.Name.TextValue);
    }

    private void DrawUi()
    {
        windows.Draw();
        toggle.Draw();
    }

    public void Dispose()
    {
        Framework.Update -= OnUpdate;
        PluginInterface.UiBuilder.Draw -= DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleSettings;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleSettings;
        CommandManager.RemoveHandler(Command);

        obs.Dispose();
        addons.RestoreAll();
        addons.Dispose();
        chat.Dispose();
        namePlates.Dispose();
        NamePlateGui.RequestRedraw();
        windows.RemoveAllWindows();
    }
}
