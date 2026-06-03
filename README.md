# Tournament Manager

A desktop app for managing battle royale tournaments. Supports multiple tournaments, 15 teams per tournament, 5 games per tournament, with automatic points calculation.

## Requirements

- [Node.js](https://nodejs.org/) v18 or higher

## Install & Run

```bash
git clone https://github.com/Cepssss/tournament-repo.git
cd tournament-repo
npm install
npm start
```

## Scoring System

Points per game = **kills × position multiplier**

| Position  | Multiplier |
|-----------|-----------|
| 1st       | ×1.6      |
| 2nd–5th   | ×1.4      |
| 6th–10th  | ×1.2      |
| 11th–15th | ×1.0      |

The winner is the team with the most accumulated points across all 5 games.

## Build Installers

```bash
npm run build:win    # Windows (.exe installer)
npm run build:mac    # macOS (.dmg)
npm run build:linux  # Linux (.AppImage)
```

Built files are output to the `dist/` folder.

## Data Storage

Tournament data is saved automatically to your OS user data directory as `tournaments.json`. No database or server required.
