# Tournament Manager

A Windows desktop app for managing battle royale tournaments. 15 teams, 5 games, automatic points calculation.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows)

## Install & Run

```bat
git clone https://github.com/Cepssss/tournament-repo.git
cd tournament-repo\TournamentManager
dotnet run
```

## Scoring System

Points per game = **kills × position multiplier**

| Position  | Multiplier |
|-----------|-----------|
| 1st       | ×1.6      |
| 2nd–5th   | ×1.4      |
| 6th–10th  | ×1.2      |
| 11th–15th | ×1.0      |

Winner = team with most total points across all 5 games.

## Build a standalone .exe

```bat
cd TournamentManager
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output goes to `bin\Release\net8.0-windows\win-x64\publish\TournamentManager.exe` — no .NET install needed to run it.

## Data

Tournament data is saved automatically to `%APPDATA%\TournamentManager\tournaments.json`.
