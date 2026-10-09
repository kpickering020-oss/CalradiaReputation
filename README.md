# Calradia Reputation

Standalone single-player Mount & Blade II: Bannerlord mod. The player earns **one** nickname from how they actually live: battles, raids, mercy, trade, age, and failure.

Built against **Bannerlord v1.4.8** using official TaleWorlds assemblies. No Harmony, ButterLib, or MCM.

## What you can try in-game

- Fight, raid, trade, and rule as usual. Reputation is scored in the background.
- Talk to a **companion in your party** and choose **What do the troops think of me?**
- Lords and notables may mention a known nickname, with a seven-day cooldown.
- Nicknames are not announced when earned. You hear them from people.

## Build

Requires the .NET SDK and a PC Bannerlord install.

```powershell
dotnet test
dotnet build -c Release
```

If MSBuild cannot find the game, set `BANNERLORD_GAME_DIR` or create `BannerlordPath.txt` in the repo root containing the game folder path.

## Deploy

```powershell
.\scripts\Deploy.ps1
```

The script looks for the game via `BANNERLORD_GAME_DIR`, `BannerlordPath.txt`, Steam `libraryfolders.vdf`, then common install folders. It does not assume a single Steam path.

Enable **Calradia Reputation** in the launcher.

## Architecture

- `CalradiaReputation.Core` — scoring, nicknames, save packing, dialogue text. Unit-tested without the game.
- `CalradiaReputation` — campaign events, `SyncData` persistence, companion/NPC dialogue.
- Starter catalog: 10 titles. The claim-score engine is ready for a larger compiled catalog.

## Manual checks (not automated)

- Load a campaign, fight a battle, save, reload, and confirm a companion still tells the same story.
- Confirm vanilla greeting flow still works if an NPC does not know your name.
