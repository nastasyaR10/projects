const $ = (id) => document.getElementById(id);

const LINES = {
  sokolnicheskaya:              { color: '#e42313', name: 'Сокольническая' },
  zamoskvoretskaya:             { color: '#2e9e44', name: 'Замоскворецкая' },
  arbatsko_pokrovskaya:         { color: '#0072ba', name: 'Арбатско-Покровская' },
  filevskaya:                   { color: '#00a3e0', name: 'Филёвская' },
  koltsevaya:                   { color: '#8b5a2b', name: 'Кольцевая' },
  kaluzhsko_rizhskaya:          { color: '#f58220', name: 'Калужско-Рижская' },
  tagansko_krasnopresnenskaya:  { color: '#8e479c', name: 'Таганско-Краснопресненская' },
  kalininskaya:                 { color: '#f7d417', name: 'Калининская' },
  solntsevskaya:                { color: '#f7d417', name: 'Солнцевская' },
  serpukhovsko_timiryazevskaya: { color: '#8a8c8e', name: 'Серпуховско-Тимирязевская' },
  lyublinsko_dmitrovskaya:      { color: '#a8c93a', name: 'Люблинско-Дмитровская' },
  bolshaya_koltsevaya:          { color: '#3fb6a8', name: 'Большая кольцевая' },
  butovskaya:                   { color: '#b3b9c1', name: 'Бутовская' },
  nekrasovskaya:                { color: '#e05a8a', name: 'Некрасовская' },
  troitskaya:                   { color: '#3d9b4f', name: 'Троицкая' },
  rublyovo_arkhangelskaya:      { color: '#a8b0b8', name: 'Рублёво-Архангельская' },
};
const lineColor = (id) => (LINES[id] && LINES[id].color) || '#9b8f79';
const lineName  = (id) => (LINES[id] && LINES[id].name)  || id;

const MODE_NAMES = { fast: 'быстрее', fewest_transfers: 'меньше пересадок' };
const modeName = (m) => MODE_NAMES[m] || m;

function getUserId() {
  let id = localStorage.getItem('userId');
  if (!id) {
    id = String(Date.now()) + String(Math.floor(Math.random() * 1000));
    localStorage.setItem('userId', id);
  }
  return id;
}

let stations = [];
const idToName = new Map();

async function loadStations() {
  const res = await fetch('/api/stations');
  if (!res.ok) throw new Error('stations: HTTP ' + res.status);
  stations = await res.json();
  stations.sort((a, b) => a.name.localeCompare(b.name, 'ru'));
  for (const s of stations) idToName.set(s.id, s.name);
}

function findByPrefix(query, limit = 10) {
  const q = query.trim().toLowerCase();
  if (!q) return [];
  return stations
    .filter((s) => s.name.toLowerCase().startsWith(q))
    .slice(0, limit);
}

function setupAutocomplete(inputId) {
  const input = $(inputId);
  const host = input.parentElement;     
  const box = document.createElement('div');
  box.className = 'suggest';
  box.hidden = true;
  host.appendChild(box);

  const render = () => {
    const matches = findByPrefix(input.value);
    box.textContent = '';
    if (matches.length === 0) { box.hidden = true; return; }

    for (const s of matches) {
      const item = document.createElement('button');
      item.type = 'button';
      item.className = 'suggest-item';
      item.innerHTML =
        `<span class="dot" style="background:${lineColor(s.line_id)}"></span>` +
        `<span>${s.name}</span>` +
        `<span class="ln">${lineName(s.line_id)}</span>`;
      item.addEventListener('mousedown', (e) => {
        e.preventDefault();               // не даём инпуту потерять фокус
        input.value = s.name;
        input.dataset.stationId = s.id;
        box.hidden = true;
      });
      box.appendChild(item);
    }
    box.hidden = false;
  };

  input.addEventListener('input', () => {
    delete input.dataset.stationId;
    render();
  });
  input.addEventListener('focus', render);
  input.addEventListener('blur', () => setTimeout(() => { box.hidden = true; }, 120));
}

function resolveStationId(input) {
  if (input.dataset.stationId) return input.dataset.stationId;
  const v = input.value.trim().toLowerCase();
  if (!v) return null;
  const byName = stations.find((s) => s.name.toLowerCase() === v);
  if (byName) return byName.id;
  const byId = stations.find((s) => s.id.toLowerCase() === v);
  return byId ? byId.id : null;
}

async function buildRoute(ev) {
  ev.preventDefault();
  const box = $('result');
  box.textContent = '';

  const fromId = resolveStationId($('from'));
  const toId = resolveStationId($('to'));
  if (!fromId || !toId) {
    box.innerHTML = '<p class="error">Станция не найдена — выберите из подсказок.</p>';
    return;
  }

  const body = { from: fromId, to: toId, mode: $('mode').value };

  let res;
  try {
    res = await fetch('/api/route', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'X-User-ID': getUserId() },
      body: JSON.stringify(body),
    });
  } catch (e) {
    box.innerHTML = `<p class="error">Сеть: ${e.message}</p>`;
    return;
  }

  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    box.innerHTML = `<p class="error">${data.error || ('HTTP ' + res.status)}</p>`;
    return;
  }
  renderRoute(data);
}

function renderRoute(r) {
  const box = $('result');
  box.textContent = '';

  const head = document.createElement('div');
  head.className = 'route-head';
  head.innerHTML =
    `<span><b>${r.total_time_min}</b> мин</span>` +
    `<span>пересадок:  <b>${r.transfers_count}</b></span>` +
    `<span class="accent">режим:  <b>${modeName(r.mode)}</b></span>`;
  box.appendChild(head);

  for (const seg of r.segments) {
    if (seg.is_transfer) {
      const el = document.createElement('div');
      el.className = 'seg transfer';
      el.innerHTML = `<span class="t">↔ пересадка · ${seg.time_min} мин</span>`;
      box.appendChild(el);
      continue;
    }
    if (seg.stations.length === 0) continue;

    const el = document.createElement('div');
    el.className = 'seg';

    const dot = document.createElement('span');
    dot.className = 'dot';
    dot.style.background = lineColor(seg.stations[0].line_id);

    const names = document.createElement('span');
    names.className = 'names';
    names.textContent = seg.stations.map((s) => s.name).join(' → ');

    const time = document.createElement('span');
    time.className = 'time';
    time.textContent = seg.time_min > 0 ? `${seg.time_min} мин` : '';

    el.append(dot, names, time);
    box.appendChild(el);
  }
}

async function loadHistory() {
  const userId = getUserId();
  const ul = $('history');
  ul.textContent = '';

  let res;
  try {
    res = await fetch(`/api/history/${userId}`, { headers: { 'X-User-ID': userId } });
  } catch (e) {
    const li = document.createElement('li');
    li.className = 'error';
    li.textContent = 'Сеть: ' + e.message;
    ul.appendChild(li);
    return;
  }

  const data = await res.json().catch(() => []);
  if (!res.ok) {
    const li = document.createElement('li');
    li.className = 'error';
    li.textContent = data.error || ('HTTP ' + res.status);
    ul.appendChild(li);
    return;
  }
  if (data.length === 0) {
    const li = document.createElement('li');
    li.className = 'muted';
    li.textContent = 'Пока пусто.';
    ul.appendChild(li);
    return;
  }

  for (const h of data) {
    const li = document.createElement('li');

    const line = document.createElement('div');
    line.className = 'route-line';
    line.innerHTML =
      `<span>${idToName.get(h.from_station) || h.from_station}</span>` +
      `<span class="arrow">→</span>` +
      `<span>${idToName.get(h.to_station) || h.to_station}</span>`;

    const meta = document.createElement('div');
    meta.className = 'meta';
    meta.textContent = `${modeName(h.mode)} · ${new Date(h.created_at).toLocaleString('ru')}`;

    li.append(line, meta);
    ul.appendChild(li);
  }
}

document.addEventListener('DOMContentLoaded', async () => {
  try {
    await loadStations();
  } catch (e) {
    $('result').innerHTML = `<p class="error">Не удалось загрузить станции: ${e.message}</p>`;
    return;
  }
  setupAutocomplete('from');
  setupAutocomplete('to');
  $('route-form').addEventListener('submit', buildRoute);
  $('history-btn').addEventListener('click', loadHistory);
});