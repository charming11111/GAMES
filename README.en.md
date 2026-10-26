<p align="center"><img src="app.png" width="88" alt="GAMES icon"></p>

<h1 align="center">GAMES</h1>
<p align="center">A lightweight local game launcher for Windows</p>
<p align="center"><a href="README.md">简体中文</a> · <strong>English</strong></p>

GAMES is built with C#, WPF and .NET Framework. It presents your local games in a
cover grid with keyboard, mouse, Xbox and PlayStation controller navigation.
No Electron or browser runtime is required. The documentation is bilingual;
the application interface currently uses Simplified Chinese.

## Windows downloads

Visit **[Releases](https://github.com/charming11111/GAMES/releases/latest)**:

- `GAMES.exe`: a standalone executable.
- `GAMES-v1.0.0-windows-x64.zip`: the executable, documentation and licenses.

Requirements: **Windows 10 / 11 x64 and .NET Framework 4.8**.
The executable is not commercially code-signed. Download from this repository's
Releases and compare its SHA-256 with the supplied checksum file.

## Features

- Multiple scan locations; scan at startup or manually with **F5** / the scan button.
- Installation-directory detection, local Epic installation manifests, utility
  exclusions and duplicate-installation merging.
- Online cover lookup, local cover selection, manual Steam App IDs and Chinese
  game-name matching where available.
- All games, favorites and recently played filters, plus name, added date,
  last-played and play-count sorting.
- Dark, light and system themes; animated gradient selection borders.
- Windowed and fullscreen modes with a saved startup preference;
  fullscreen uses nine cards per row.
- Xbox / PlayStation glyphs and controller navigation. Background controller
  input is ignored, and controls must return to neutral when focus returns.
- Five synthesized sound styles with previews and separate volume presets.
- System-tray minimization and a game launch waiting overlay.

## Getting started

1. Run `GAMES.exe`, then open **设置 → 游戏扫描目录 → 添加扫描目录**
   (Settings → Scan locations → Add location).
2. Select an installation directory or drive. Multiple locations are supported.
3. Wait for scanning and cover lookup. Restart or press **F5** after adding or removing games.
4. Confirm a card to open its menu: launch, favorite, refresh cover, open its
   directory or manually choose its executable.

No personal drive or installation paths are preconfigured. Removing a scan location
does not delete games. Removing a game only hides the entry; it can be restored
under **已移除游戏** in Settings. Cover lookup requires the Internet; local launch does not.

### Controls

| Action | Keyboard | Controller |
| --- | --- | --- |
| Navigate | Arrow keys, Tab | D-pad / left stick |
| Confirm / game menu | Enter | A / × |
| Back | Esc | B / ○ |
| Settings | F1 | Menu / Options |
| Toggle fullscreen | F11 | Settings menu |
| Scan now | F5 | Top scan button |
| Search | Ctrl+F / magnifier | Navigate to search and confirm |
| Favorite | Game menu | Y / △ |
| Filter | Filter button | X / □ |
| Page | Mouse wheel | LB/RB, L1/R1 |

Controller mappings can vary with hardware, connection type and drivers.

## Build from source

On Windows x64, using Windows PowerShell or PowerShell 7:

```powershell
git clone https://github.com/charming11111/GAMES.git
cd GAMES
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -Test
```

The output is `out\GAMES.exe`. The script uses the system .NET Framework compiler;
Visual Studio, Node.js and additional NuGet packages are not required.
`-Test` runs regression checks in an isolated temporary directory and writes
`out\test-result.txt`. These checks do not certify every game, display or controller.

The bundled `SDL2.dll` is embedded in the executable and extracted to the application
data directory on first use. Its license is embedded and included in the ZIP release.

## Limitations and local data

- Executable detection uses heuristics, a small set of generic entry profiles and
  platform manifests. It is not guaranteed for every game. For unusual launchers,
  rendering backends or anti-cheat entries, choose the EXE manually in the game menu.
- Only configured directories are scanned; the app does not scan every drive or
  require a folder of game shortcuts.
- Epic DLC is not listed as a separate installed game. Owned-but-uninstalled games
  are outside the local library's scope.
- Cover lookup can fail or return incorrect artwork. Use a local image or a manual
  Steam App ID to correct it.
- The animation target is 144 frames per second, **not a measured FPS guarantee**.
- Preferences, cached artwork and local error logs are stored in
  `%LOCALAPPDATA%\GameShelf`. Cover requests send game search terms to the relevant
  store or image provider. No platform password or token is required.

## Source map

| Files | Purpose |
| --- | --- |
| `Shell.cs`, `Visuals.cs`, `CompactHeader.cs` | Window and layout |
| `Discovery.cs`, `entry-profiles.json` | Discovery, deduplication and entry profiles |
| `Controllers.cs`, `Navigation.cs` | Controller and keyboard navigation |
| `Metadata.cs`, `Cache.cs` | Covers and metadata |
| `Audio.cs` | Synthesized UI audio |
| `LaunchWatch.cs`, `Tray.cs` | Launch detection and tray integration |
| `ContinuousScroll.cs`, `Performance.cs` | Scrolling and resource reuse |
| `Tests.cs`, `build.ps1` | Regression checks and build |

Issues and pull requests are welcome. For detection problems, share game names,
directory structure and candidate EXE names. Do not publish private paths, account
information, tokens or full game files.

## License

Project source is licensed under [MIT](LICENSE). SDL2 uses the zlib license.
Game artwork, trademarks and store metadata are not covered by this project's MIT
license. See [Third-party notices](THIRD-PARTY-NOTICES.md).
