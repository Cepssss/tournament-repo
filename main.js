const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const fs = require('fs');

let DATA_FILE;

function getDataFile() {
  if (!DATA_FILE) {
    DATA_FILE = path.join(app.getPath('userData'), 'tournaments.json');
    if (!fs.existsSync(DATA_FILE)) {
      fs.writeFileSync(DATA_FILE, JSON.stringify({ tournaments: [] }, null, 2));
    }
  }
  return DATA_FILE;
}

function load() {
  return JSON.parse(fs.readFileSync(getDataFile(), 'utf-8'));
}

function save(data) {
  fs.writeFileSync(getDataFile(), JSON.stringify(data, null, 2));
}

function createWindow() {
  const win = new BrowserWindow({
    width: 1280,
    height: 800,
    minWidth: 960,
    minHeight: 600,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false
    }
  });
  win.loadFile(path.join(__dirname, 'src', 'index.html'));
}

app.whenReady().then(() => {
  createWindow();
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});

ipcMain.handle('get-tournaments', () => load().tournaments);

ipcMain.handle('create-tournament', (_, { name, teams }) => {
  const data = load();
  const tournament = {
    id: Date.now().toString(),
    name,
    createdAt: new Date().toISOString(),
    teams,
    games: []
  };
  data.tournaments.push(tournament);
  save(data);
  return tournament;
});

ipcMain.handle('add-game', (_, { tournamentId, results }) => {
  const data = load();
  const t = data.tournaments.find(x => x.id === tournamentId);
  if (!t) return null;
  const game = { id: t.games.length + 1, playedAt: new Date().toISOString(), results };
  t.games.push(game);
  save(data);
  return game;
});

ipcMain.handle('update-game', (_, { tournamentId, gameId, results }) => {
  const data = load();
  const t = data.tournaments.find(x => x.id === tournamentId);
  if (!t) return null;
  const game = t.games.find(g => g.id === gameId);
  if (!game) return null;
  game.results = results;
  save(data);
  return game;
});

ipcMain.handle('delete-game', (_, { tournamentId, gameId }) => {
  const data = load();
  const t = data.tournaments.find(x => x.id === tournamentId);
  if (!t) return null;
  t.games = t.games.filter(g => g.id !== gameId);
  t.games.forEach((g, i) => { g.id = i + 1; });
  save(data);
  return true;
});

ipcMain.handle('delete-tournament', (_, id) => {
  const data = load();
  data.tournaments = data.tournaments.filter(t => t.id !== id);
  save(data);
  return true;
});
