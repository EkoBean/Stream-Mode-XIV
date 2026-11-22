# Stream Mode XIV

Dalamud plugin (Dalamud.NET.Sdk 15, net10.0-windows) that abbreviates player names (Bean Annyx -> B.A) across the UI while streaming, and turns on/off with OBS.

## Build
`dotnet build` from the repo root. The DLL is always at `StreamModeXIV\bin\Debug\StreamModeXIV.dll` (Release: `bin\Release`). `AppendPlatformToOutputPath=false` keeps the path identical whether the solution or the project is built; do not remove it. Register that DLL path as a Dalamud dev plugin.

## Where settings live (one place only)
- Plugin config: `%APPDATA%\XIVLauncher\pluginConfigs\StreamModeXIV.json` via `SavePluginConfig`. Same file for dev and installed builds.
- The on-screen toggle uses `ImGuiWindowFlags.NoSavedSettings`, so its position is never written to an imgui `.ini`; config is the only source of truth. Do not add other ini/config files.

## Layout
- `Plugin.cs` services, command `/streammode [on|off|toggle]`, `SetStreamMode` = single switch path.
- `ObsWatcher.cs` polls for obs64/obs32 processes.
- `Windows/ToggleOverlay.cs` the bare "Stream Mode (O  )" switch; `Windows/SettingsWindow.cs`.
- `Replacers/` NamePlate, Chat, AddonText (addon lists + rules). See `docs/name-locations.md`.
