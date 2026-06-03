// ===== Constants =====
const TOTAL_TEAMS = 15;
const TOTAL_GAMES = 5;

const MULTIPLIERS = {
  1:  1.6,
  2:  1.4, 3:  1.4, 4:  1.4, 5:  1.4,
  6:  1.2, 7:  1.2, 8:  1.2, 9:  1.2, 10: 1.2,
  11: 1.0, 12: 1.0, 13: 1.0, 14: 1.0, 15: 1.0
};

function getMultiplier(position) {
  return MULTIPLIERS[position] ?? 1.0;
}

function calcScore(kills, position) {
  return +(kills * getMultiplier(position)).toFixed(2);
}

function fmtPts(n) {
  if (n === 0) return '0';
  return Number.isInteger(n) ? n.toString() : n.toFixed(2).replace(/\.?0+$/, '');
}

// ===== State =====
const state = {
  tournaments: [],
  currentTournamentId: null,
  editingGameId: null
};

// ===== DOM Helpers =====
const $ = id => document.getElementById(id);

function show(el) {
  const node = typeof el === 'string' ? $(el) : el;
  node.hidden = false;
}
function hide(el) {
  const node = typeof el === 'string' ? $(el) : el;
  node.hidden = true;
}

function escHtml(str) {
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

// ===== View Switching =====
function showView(name) {
  document.querySelectorAll('.view').forEach(v => v.classList.remove('active'));
  $(`view-${name}`).classList.add('active');
}

// ===== Data =====
async function loadTournaments() {
  state.tournaments = await window.api.getTournaments();
}

function getTournament(id) {
  return state.tournaments.find(t => t.id === id) ?? null;
}

// ===== Standings Calculator =====
function calcStandings(tournament) {
  const map = {};
  for (const team of tournament.teams) {
    map[team] = { team, games: [], total: 0 };
  }
  for (const game of tournament.games) {
    for (const r of game.results) {
      if (!map[r.team]) continue;
      const pts = calcScore(r.kills, r.position);
      map[r.team].games.push(pts);
      map[r.team].total = +(map[r.team].total + pts).toFixed(2);
    }
  }
  return Object.values(map).sort((a, b) => b.total - a.total);
}

// ===== Home View =====
function renderHome() {
  showView('home');
  const grid    = $('tournaments-grid');
  const empty   = $('empty-state');
  grid.innerHTML = '';

  if (state.tournaments.length === 0) {
    show(empty);
    grid.style.display = 'none';
  } else {
    hide(empty);
    grid.style.display = 'grid';
    for (const t of state.tournaments) {
      grid.appendChild(makeTournamentCard(t));
    }
  }
}

function makeTournamentCard(t) {
  const gamesPlayed = t.games.length;
  const complete    = gamesPlayed >= TOTAL_GAMES;
  const pct         = Math.round((gamesPlayed / TOTAL_GAMES) * 100);

  let statusHtml = '';
  if (gamesPlayed > 0) {
    const standings = calcStandings(t);
    const top = standings[0]?.team ?? '';
    statusHtml = `<div class="card-status ${complete ? 'complete' : 'active'}">
      ${complete ? '🏆 Winner: ' : '🔝 Leading: '}${escHtml(top)}
    </div>`;
  }

  const card = document.createElement('div');
  card.className = 'tournament-card';
  card.innerHTML = `
    <h3>${escHtml(t.name)}</h3>
    <div class="card-meta">
      <span>👥 ${t.teams.length} teams</span>
      <span>🎮 ${gamesPlayed} / ${TOTAL_GAMES} games</span>
    </div>
    <div class="progress-bar-wrap">
      <div class="progress-bar" style="width:${pct}%"></div>
    </div>
    ${statusHtml}
  `;
  card.addEventListener('click', () => openTournament(t.id));
  return card;
}

// ===== Tournament View =====
function openTournament(id) {
  state.currentTournamentId = id;
  renderTournament();
  showView('tournament');
}

function renderTournament() {
  const t = getTournament(state.currentTournamentId);
  if (!t) { renderHome(); return; }

  const gamesPlayed = t.games.length;
  const complete    = gamesPlayed >= TOTAL_GAMES;

  $('tournament-title').textContent = t.name;
  $('games-progress').textContent   = `${gamesPlayed} / ${TOTAL_GAMES} games`;
  $('btn-add-game').disabled        = complete;

  const banner = $('complete-banner');
  if (complete) {
    const standings = calcStandings(t);
    $('winner-name').textContent = standings[0]?.team ?? '';
    show(banner);
  } else {
    hide(banner);
  }

  renderStandings(t);
  renderGames(t);
}

function renderStandings(t) {
  const gamesPlayed = t.games.length;
  const standings   = calcStandings(t);

  // Header
  let headHtml = '<tr><th>#</th><th>Team</th>';
  for (let i = 1; i <= gamesPlayed; i++) headHtml += `<th style="text-align:right">G${i}</th>`;
  headHtml += '<th style="text-align:right">Total</th></tr>';
  $('standings-head').innerHTML = headHtml;

  // Body
  const tbody = $('standings-body');
  tbody.innerHTML = '';
  standings.forEach((s, idx) => {
    const rank  = idx + 1;
    const medal = rank === 1 ? '🥇' : rank === 2 ? '🥈' : rank === 3 ? '🥉' : rank;
    const tr    = document.createElement('tr');
    if (rank <= 3) tr.className = `rank-${rank}`;

    let cells = `<td class="rank-cell">${medal}</td>`;
    cells    += `<td class="team-name-cell">${escHtml(s.team)}</td>`;
    for (const pts of s.games) {
      cells += `<td class="game-pts-cell">${pts !== undefined ? fmtPts(pts) : '—'}</td>`;
    }
    cells += `<td class="total-cell">${fmtPts(s.total)}</td>`;
    tr.innerHTML = cells;
    tbody.appendChild(tr);
  });
}

function renderGames(t) {
  const list = $('games-list');
  if (t.games.length === 0) {
    list.innerHTML = '<p style="color:var(--text-muted);text-align:center;padding:24px 0">No games played yet.</p>';
    return;
  }
  list.innerHTML = '';
  for (const game of t.games) {
    list.appendChild(makeGameCard(t, game));
  }
}

function makeGameCard(tournament, game) {
  const sorted = [...game.results].sort((a, b) => a.position - b.position);

  const card = document.createElement('div');
  card.className = 'game-card';

  const rowsHtml = sorted.map(r => `
    <tr>
      <td>#${r.position}</td>
      <td class="team-name-cell">${escHtml(r.team)}</td>
      <td>${r.kills}</td>
      <td class="mult-cell">×${getMultiplier(r.position)}</td>
      <td class="total-cell">${fmtPts(calcScore(r.kills, r.position))}</td>
    </tr>
  `).join('');

  card.innerHTML = `
    <div class="game-card-header">
      <span class="game-card-title">Game ${game.id}</span>
      <div class="game-card-right">
        <div class="game-card-actions">
          <button class="btn btn-ghost btn-sm js-edit-game">Edit</button>
          <button class="btn btn-danger btn-sm js-delete-game">Delete</button>
        </div>
        <span class="expand-icon">▼</span>
      </div>
    </div>
    <div class="game-card-body">
      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr><th>Pos</th><th>Team</th><th>Kills</th><th>Mult</th><th>Points</th></tr>
          </thead>
          <tbody>${rowsHtml}</tbody>
        </table>
      </div>
    </div>
  `;

  card.querySelector('.game-card-header').addEventListener('click', () => {
    card.classList.toggle('expanded');
  });

  card.querySelector('.js-edit-game').addEventListener('click', e => {
    e.stopPropagation();
    openGameModal(game.id);
  });

  card.querySelector('.js-delete-game').addEventListener('click', e => {
    e.stopPropagation();
    confirmDeleteGame(tournament.id, game.id);
  });

  return card;
}

// ===== Game Modal =====
function openGameModal(gameId = null) {
  const t = getTournament(state.currentTournamentId);
  if (!t) return;

  state.editingGameId = gameId;
  const existing = gameId ? t.games.find(g => g.id === gameId) : null;
  const gameNum  = gameId ?? t.games.length + 1;

  $('game-modal-title').textContent = gameId ? `Edit Game ${gameId}` : `Add Game ${gameNum}`;
  hide('game-error');

  const tbody = $('game-entry-body');
  tbody.innerHTML = '';

  for (const team of t.teams) {
    const prev = existing?.results.find(r => r.team === team);
    const tr   = document.createElement('tr');
    tr.dataset.team = team;

    tr.innerHTML = `
      <td class="team-name-cell">${escHtml(team)}</td>
      <td><input class="pos-input" type="number" min="1" max="${TOTAL_TEAMS}"
           placeholder="1–${TOTAL_TEAMS}" value="${prev ? prev.position : ''}"></td>
      <td><input class="kills-input" type="number" min="0"
           placeholder="0" value="${prev ? prev.kills : ''}"></td>
      <td class="mult-cell"><span class="mult-display">—</span></td>
      <td><span class="pts-live">—</span></td>
    `;

    const posInput   = tr.querySelector('.pos-input');
    const killsInput = tr.querySelector('.kills-input');
    const multSpan   = tr.querySelector('.mult-display');
    const ptsSpan    = tr.querySelector('.pts-live');

    function updateRow() {
      const pos   = parseInt(posInput.value);
      const kills = parseInt(killsInput.value);
      if (pos >= 1 && pos <= TOTAL_TEAMS) {
        const mult = getMultiplier(pos);
        multSpan.textContent = `×${mult}`;
        ptsSpan.textContent  = (!isNaN(kills) && kills >= 0) ? fmtPts(calcScore(kills, pos)) : '—';
      } else {
        multSpan.textContent = '—';
        ptsSpan.textContent  = '—';
      }
    }

    posInput.addEventListener('input', updateRow);
    killsInput.addEventListener('input', updateRow);
    if (prev) updateRow();

    tbody.appendChild(tr);
  }

  show('modal-game');
}

async function saveGame() {
  const t = getTournament(state.currentTournamentId);
  if (!t) return;

  const rows = $('game-entry-body').querySelectorAll('tr');

  // Reset validation state
  rows.forEach(row => {
    row.querySelector('.pos-input').classList.remove('invalid');
    row.querySelector('.kills-input').classList.remove('invalid');
  });
  hide('game-error');

  const results  = [];
  let hasError   = false;

  for (const row of rows) {
    const team       = row.dataset.team;
    const posInput   = row.querySelector('.pos-input');
    const killsInput = row.querySelector('.kills-input');
    const pos        = parseInt(posInput.value);
    const kills      = parseInt(killsInput.value);
    let rowValid     = true;

    if (isNaN(pos) || pos < 1 || pos > TOTAL_TEAMS) {
      posInput.classList.add('invalid');
      hasError  = true;
      rowValid  = false;
    }
    if (isNaN(kills) || kills < 0) {
      killsInput.classList.add('invalid');
      hasError  = true;
      rowValid  = false;
    }
    if (rowValid) results.push({ team, position: pos, kills });
  }

  if (hasError) {
    showGameError(`All fields required — position 1–${TOTAL_TEAMS}, kills ≥ 0.`);
    return;
  }

  // Check duplicate positions
  const seen = new Set();
  for (const r of results) {
    if (seen.has(r.position)) {
      rows.forEach(row => {
        const inp = row.querySelector('.pos-input');
        if (parseInt(inp.value) === r.position) inp.classList.add('invalid');
      });
      showGameError(`Position ${r.position} is used by more than one team.`);
      return;
    }
    seen.add(r.position);
  }

  if (state.editingGameId) {
    await window.api.updateGame(state.currentTournamentId, state.editingGameId, results);
    const localT = getTournament(state.currentTournamentId);
    const game   = localT?.games.find(g => g.id === state.editingGameId);
    if (game) game.results = results;
  } else {
    const newGame = await window.api.addGame(state.currentTournamentId, results);
    const localT  = getTournament(state.currentTournamentId);
    if (newGame && localT) localT.games.push(newGame);
  }

  hide('modal-game');
  renderTournament();
}

function showGameError(msg) {
  const el = $('game-error');
  el.textContent = msg;
  show(el);
}

// ===== Tournament Modal =====
function openTournamentModal() {
  $('input-tournament-name').value = '';

  const grid = $('teams-grid');
  grid.innerHTML = '';
  for (let i = 1; i <= TOTAL_TEAMS; i++) {
    const input = document.createElement('input');
    input.className   = 'field-input';
    input.type        = 'text';
    input.placeholder = `Team ${i}`;
    input.maxLength   = 40;
    input.autocomplete = 'off';
    grid.appendChild(input);
  }

  show('modal-tournament');
  setTimeout(() => $('input-tournament-name').focus(), 50);
}

async function createTournament() {
  const name = $('input-tournament-name').value.trim();
  if (!name) {
    $('input-tournament-name').focus();
    return;
  }

  const inputs = $('teams-grid').querySelectorAll('input');
  const teams  = Array.from(inputs).map((inp, i) => inp.value.trim() || `Team ${i + 1}`);

  const tournament = await window.api.createTournament(name, teams);
  state.tournaments.push(tournament);

  hide('modal-tournament');
  openTournament(tournament.id);
}

// ===== Delete Operations =====
async function confirmDeleteGame(tournamentId, gameId) {
  if (!confirm(`Delete Game ${gameId}? This cannot be undone.`)) return;

  await window.api.deleteGame(tournamentId, gameId);

  const localT = getTournament(tournamentId);
  if (localT) {
    localT.games = localT.games.filter(g => g.id !== gameId);
    localT.games.forEach((g, i) => { g.id = i + 1; });
  }
  renderTournament();
}

async function confirmDeleteTournament() {
  const t = getTournament(state.currentTournamentId);
  if (!t) return;
  if (!confirm(`Delete "${t.name}"? This cannot be undone.`)) return;

  await window.api.deleteTournament(state.currentTournamentId);
  state.tournaments = state.tournaments.filter(x => x.id !== state.currentTournamentId);
  state.currentTournamentId = null;
  renderHome();
}

// ===== Bootstrap =====
document.addEventListener('DOMContentLoaded', async () => {
  await loadTournaments();
  renderHome();

  // Home
  $('btn-new-tournament').addEventListener('click', openTournamentModal);
  $('btn-new-tournament-empty').addEventListener('click', openTournamentModal);

  // Tournament view
  $('btn-back').addEventListener('click', async () => {
    await loadTournaments();
    renderHome();
  });
  $('btn-add-game').addEventListener('click', () => openGameModal(null));
  $('btn-delete-tournament').addEventListener('click', confirmDeleteTournament);

  // Tournament modal
  $('btn-create-tournament').addEventListener('click', createTournament);
  $('input-tournament-name').addEventListener('keydown', e => {
    if (e.key === 'Enter') createTournament();
  });

  // Game modal
  $('btn-save-game').addEventListener('click', saveGame);

  // Close buttons (data-close attribute)
  document.querySelectorAll('[data-close]').forEach(btn => {
    btn.addEventListener('click', () => hide(btn.dataset.close));
  });

  // Close overlay on backdrop click
  document.querySelectorAll('.overlay').forEach(overlay => {
    overlay.addEventListener('click', e => {
      if (e.target === overlay) hide(overlay);
    });
  });
});
