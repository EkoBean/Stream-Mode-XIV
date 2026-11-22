# Stream Mode XIV

Abbreviate every player name on your FFXIV screen while you stream.

**Jane Doe -> J.D**

The game's built-in name abbreviation only covers the party list and nameplates. Stream Mode XIV also covers the Social window, Party Finder, Adventurer Plates, the target bar and more, and it turns itself on and off with OBS, so there is nothing to set up each time you go live.

---

## Features

### Where names are abbreviated

While Stream Mode is on, player names in these places are shown as initials:

| Category | Locations |
|---|---|
| On screen | Nameplates, Party List, Alliance List, target bar (including the player a boss is targeting), focus target |
| Social | Party Members, Friend List, Player Search results |
| Lists | Blacklist, Mute List, Contacts |
| Groups | Free Company, Linkshell, Cross-world Linkshell, Fellowship |
| Character info | Adventurer Plate, Character window, Examine |
| Recruitment | Party Finder recruiter names (descriptions, duty names and tags are left as they are) |
| Other | Character select screen, Ready Check, Trade, context menus, NPC dialogue that uses your name, PvP scoreboards, crafter signatures on items, housing placards |
| Chat log | Optional, toggle it in the settings |

Game text such as tab names, duty names, place names, titles and boss names is never abbreviated by mistake.

### On-screen toggle

```
Stream Mode (O  )
```

The toggle sits directly on your game screen: a single switch, no window around it. Click it to switch Stream Mode; it turns green when on.

Set its position by typing X / Y coordinates in the settings, or turn on drag mode and move it with the mouse.

### Follows OBS automatically

- **OBS starts** → Stream Mode turns on. If the toggle isn't on screen, the settings window opens so you can confirm.
- **OBS closes** → Stream Mode turns off and names go back to normal.

This also works if OBS is already running when you start the game. Both behaviors can be turned off separately in the settings.

---

## In-game Command

| Command | Action |
|---|---|
| `/streammode` | Open the settings window |
| `/streammode on` | Turn Stream Mode on |
| `/streammode off` | Turn Stream Mode off |
| `/streammode toggle` | Switch Stream Mode |

You can also open the settings from the gear icon in Dalamud's plugin list.
