# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from `TournamentManager/`:

```bat
dotnet build                  # debug build
dotnet run                    # run in development
dotnet build -c Release       # release build

# Standalone .exe (no .NET required on target machine)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
# Output: bin\Release\net8.0-windows\win-x64\publish\TournamentManager.exe
```

There are no tests. No linter beyond the C# compiler (`Nullable enable`, `ImplicitUsings enable`).

## Architecture

**Single-project .NET 8 Windows Forms app.** No external NuGet packages — only the BCL and `System.Text.Json` (built-in).

### Data flow

`DataService` reads/writes `%APPDATA%\TournamentManager\tournaments.json` on every load/save — there is no in-memory cache. Every form that mutates data calls `_ds.Load()` → mutate → `_ds.Save()` before calling `RefreshView()`. There is no shared app-level state object.

### Scoring

All score maths live in `Helpers/ScoreCalculator.cs`. Scores are **never stored** — they are recalculated from raw `Kills` + `Position` on every `CalcStandings()` call. Multipliers are stored per-tournament (`Tournament.Multipliers`, a `List<double>` of 15 values, index 0 = 1st place). Old tournaments without a `Multipliers` field in JSON fall back to `ScoreCalculator.DefaultMultipliers` via the property initializer.

Formula: `score = kills × multipliers[position - 1]`

### Key relationships

- `MainForm` owns `DataService` and passes a `Tournament` reference + `DataService` to `TournamentForm`.
- `TournamentForm` re-loads the full list on every `Persist()` call, finds the current tournament by `Id`, replaces it, and saves. This means the `Tournament` object held by `TournamentForm` is the authoritative in-memory copy for the duration of that dialog.
- `GameEntryForm` and `MultipliersForm` are modal dialogs — they return results via public properties (`Results`, `Result`) and the caller applies changes.
- `SplitContainer` min sizes and `SplitterDistance` in `TournamentForm` must be set inside the `Load` event, not during construction, to avoid an `InvalidOperationException` when the control has no size yet.

### UI conventions

- All colours, button factory (`AppColors.Btn`), and `DataGridView` styling (`AppColors.StyleDgv`) are centralised in `Helpers/AppColors.cs`.
- Forms are built entirely in code — no `.Designer.cs` files.
- Dark theme: `AppColors.Background` → `AppColors.Surface` → `AppColors.Surface2` (light to dark hierarchy is inverted — Background is darkest).
