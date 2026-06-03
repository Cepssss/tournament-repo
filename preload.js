const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('api', {
  getTournaments: () => ipcRenderer.invoke('get-tournaments'),
  createTournament: (name, teams) => ipcRenderer.invoke('create-tournament', { name, teams }),
  addGame: (tournamentId, results) => ipcRenderer.invoke('add-game', { tournamentId, results }),
  updateGame: (tournamentId, gameId, results) => ipcRenderer.invoke('update-game', { tournamentId, gameId, results }),
  deleteGame: (tournamentId, gameId) => ipcRenderer.invoke('delete-game', { tournamentId, gameId }),
  deleteTournament: (id) => ipcRenderer.invoke('delete-tournament', id)
});
