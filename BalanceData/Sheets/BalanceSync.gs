/**
 * Flames of Faith - balance sheet sync.
 *
 * Paste this whole file into the balance workbook (Extensions > Apps Script), save, and deploy it once as a web app
 * (Deploy > New deployment > Web app: Execute as "Me", Who has access "Anyone"). Then Balance > Sync settings
 * (owner only) shows the token Unity and the test builds need.
 *
 * - Balance > Publish freezes the current data tabs as a numbered version, with a note saying what changed. Unity's
 *   "Pull from Sheet" and the test builds always read the latest published version, never half-finished edits.
 * - The web app (doGet / doPost) serves the published versions to Unity and the test builds, and lets Unity write
 *   the tabs back: the Report_ tabs after every import, and every data tab on "Push All to Sheet".
 *
 * Every request needs the token. Published versions live in the script's properties (the last 5 are kept).
 */

const README_TAB = 'Read Me';
const REPORT_PREFIX = 'Report_';
const KEEP_VERSIONS = 5;
const CHUNK_SIZE = 8000; // script properties hold at most 9 KB per value

// --- Menu -------------------------------------------------------------------------------------------------

function onOpen() {
  SpreadsheetApp.getUi()
    .createMenu('Balance')
    .addItem('Publish...', 'publish')
    .addItem('Last published version', 'showLastPublished')
    .addSeparator()
    .addItem('Sync settings (owner)', 'showSyncSettings')
    .addToUi();
}

function publish() {
  const ui = SpreadsheetApp.getUi();
  const answer = ui.prompt('Publish balance data',
    'What changed? This note is shown in the test build and kept in the game\'s history.', ui.ButtonSet.OK_CANCEL);
  if (answer.getSelectedButton() !== ui.Button.OK) return;

  const lock = LockService.getScriptLock();
  lock.waitLock(30000);
  try {
    const props = PropertiesService.getScriptProperties();
    const snapshot = buildSnapshot_();
    const last = latestMeta_(props);
    if (last && last.hash === snapshot.hash) {
      ui.alert('Nothing changed since version ' + last.version + ', so nothing was published.');
      return;
    }
    snapshot.version = last ? last.version + 1 : 1;
    snapshot.publishedAt = new Date().toISOString();
    snapshot.author = Session.getActiveUser().getEmail() || 'unknown';
    snapshot.note = answer.getResponseText().trim();
    saveSnapshot_(props, snapshot);
    ui.alert('Published version ' + snapshot.version + '.\n\nRestart the test build to play it.');
  } finally {
    lock.releaseLock();
  }
}

function showLastPublished() {
  const last = latestMeta_(PropertiesService.getScriptProperties());
  SpreadsheetApp.getUi().alert(last
    ? 'Version ' + last.version + ', published ' + last.publishedAt + ' by ' + last.author + '.\n\n' + (last.note || '(no note)')
    : 'Nothing has been published yet.');
}

function showSyncSettings() {
  const ui = SpreadsheetApp.getUi();
  const spreadsheet = SpreadsheetApp.getActive();
  const owner = spreadsheet.getOwner();
  if (!owner || Session.getActiveUser().getEmail() !== owner.getEmail()) {
    ui.alert('Only the owner of this workbook can see the sync settings.');
    return;
  }
  const props = PropertiesService.getScriptProperties();
  let token = props.getProperty('token');
  if (!token) {
    token = Utilities.getUuid().replace(/-/g, '');
    props.setProperty('token', token);
  }
  ui.alert('Sync settings',
    'Token: ' + token + '\n\n' +
    'Web app URL: copy the ".../exec" URL from Deploy > Manage deployments.\n\n' +
    'In Unity: Tools > Flames of Faith > Balance Data > Sheet Settings.\n' +
    'Test builds: a balance-override.json file next to the game\'s .exe with the same URL and token.',
    ui.ButtonSet.OK);
}

// --- Snapshots ----------------------------------------------------------------------------------------------

// Every tab but the Read Me, the reports, and the designer's own scratch tabs (named with a leading "_")
function dataTabs_() {
  return SpreadsheetApp.getActive().getSheets()
    .filter(s => s.getName() !== README_TAB && s.getName().indexOf(REPORT_PREFIX) !== 0 && s.getName().indexOf('_') !== 0);
}

function cellText_(value) {
  if (value === true) return 'TRUE';
  if (value === false) return 'FALSE';
  if (typeof value === 'number') return String(value);
  if (value instanceof Date) return Utilities.formatDate(value, 'UTC', 'yyyy-MM-dd');
  return String(value).trim();
}

function csvCell_(text) {
  return /[",\r\n]/.test(text) || text !== text.trim() ? '"' + text.replace(/"/g, '""') + '"' : text;
}

// The table's width: the header cells up to the first empty one. What sits right of that gap is the explanation panel
// (writeNotes_), which is not data.
function headerWidth_(sheet) {
  if (sheet.getLastColumn() === 0) return 0;
  const header = sheet.getRange(1, 1, 1, sheet.getLastColumn()).getValues()[0];
  let width = 0;
  while (width < header.length && cellText_(header[width]) !== '') width++;
  return width;
}

// A tab as CSV: the header's columns only, and only rows whose first cell has something in it (spare rows with
// empty checkboxes are skipped). Null for a tab with no header row.
function tabCsv_(sheet) {
  const values = sheet.getDataRange().getValues();
  if (values.length === 0) return null;
  const width = headerWidth_(sheet);
  if (width === 0) return null;

  const lines = [];
  values.forEach((row, index) => {
    if (index > 0 && cellText_(row[0]) === '') return;
    lines.push(row.slice(0, width).map(v => csvCell_(cellText_(v))).join(','));
  });
  return lines.join('\n') + '\n';
}

function buildSnapshot_() {
  const tables = dataTabs_().map(s => ({ name: s.getName(), csv: tabCsv_(s) })).filter(t => t.csv !== null);
  const digest = Utilities.computeDigest(Utilities.DigestAlgorithm.SHA_256,
    tables.map(t => t.name + '\n' + t.csv).join('\n'), Utilities.Charset.UTF_8);
  const hash = digest.map(b => ('0' + (b & 0xff).toString(16)).slice(-2)).join('');
  return { tables: tables, hash: hash };
}

function latestMeta_(props) {
  const json = props.getProperty('latest');
  return json ? JSON.parse(json) : null;
}

function saveSnapshot_(props, snapshot) {
  const json = JSON.stringify(snapshot);
  const values = {};
  let count = 0;
  for (let i = 0; i < json.length; i += CHUNK_SIZE) {
    values['v' + snapshot.version + '_' + count] = json.substring(i, i + CHUNK_SIZE);
    count++;
  }
  values['v' + snapshot.version + '_count'] = String(count);
  values['latest'] = JSON.stringify({
    version: snapshot.version, publishedAt: snapshot.publishedAt, author: snapshot.author,
    note: snapshot.note, hash: snapshot.hash,
  });
  props.setProperties(values);

  // Forget versions older than the last few
  const old = snapshot.version - KEEP_VERSIONS;
  if (old >= 1) {
    const oldCount = Number(props.getProperty('v' + old + '_count') || 0);
    for (let i = 0; i < oldCount; i++) props.deleteProperty('v' + old + '_' + i);
    props.deleteProperty('v' + old + '_count');
  }
}

function loadSnapshot_(props, version) {
  const count = Number(props.getProperty('v' + version + '_count') || 0);
  if (count === 0) return null;
  let json = '';
  for (let i = 0; i < count; i++) json += props.getProperty('v' + version + '_' + i);
  return json;
}

// --- Web app ------------------------------------------------------------------------------------------------

function respond_(object) {
  return ContentService.createTextOutput(typeof object === 'string' ? object : JSON.stringify(object))
    .setMimeType(ContentService.MimeType.JSON);
}

function authorized_(token) {
  const expected = PropertiesService.getScriptProperties().getProperty('token');
  return !!expected && token === expected;
}

// GET ?token=...&action=latest (the latest published version), action=version&version=N, or action=ping.
function doGet(e) {
  const params = e.parameter || {};
  if (!authorized_(params.token)) return respond_({ error: 'wrong or missing token' });
  const props = PropertiesService.getScriptProperties();
  const last = latestMeta_(props);

  if (params.action === 'ping') return respond_({ ok: true, latest: last });
  const version = params.action === 'version' ? Number(params.version) : (last ? last.version : 0);
  const json = version ? loadSnapshot_(props, version) : null;
  return respond_(json || { error: version ? 'version ' + version + ' is not kept any more' : 'nothing has been published yet' });
}

// POST {token, action: "reports" | "tables" | "notes", tables: [{name, csv, description, columnNotes}]}: writes the
// given tabs. "reports" only accepts Report_ tabs (creating them if needed); "tables" also overwrites data tabs,
// keeping their formatting and checks, and rewrites their explanation panel; "notes" rewrites only the panel.
function doPost(e) {
  let body;
  try {
    body = JSON.parse(e.postData.contents);
  } catch (error) {
    return respond_({ error: 'the request is not JSON' });
  }
  if (!authorized_(body.token)) return respond_({ error: 'wrong or missing token' });
  if (body.action !== 'reports' && body.action !== 'tables' && body.action !== 'notes') return respond_({ error: 'unknown action ' + body.action });

  const lock = LockService.getScriptLock();
  lock.waitLock(30000);
  try {
    const written = [];
    (body.tables || []).forEach(table => {
      const isReport = table.name.indexOf(REPORT_PREFIX) === 0;
      if (body.action === 'notes') {
        const sheet = SpreadsheetApp.getActive().getSheetByName(table.name);
        if (!sheet || isReport) throw new Error('there is no data tab ' + table.name);
        writeNotes_(sheet, headerWidth_(sheet), table.description, table.columnNotes);
      } else {
        if (!isReport && body.action !== 'tables') throw new Error(table.name + ' is not a report');
        writeTab_(table.name, table.csv, isReport, table.description, table.columnNotes);
      }
      written.push(table.name);
    });
    return respond_({ ok: true, written: written });
  } catch (error) {
    return respond_({ error: String(error.message || error) });
  } finally {
    lock.releaseLock();
  }
}

function typed_(text) {
  if (text === 'TRUE') return true;
  if (text === 'FALSE') return false;
  if (/^-?\d+(\.\d+)?([eE][-+]?\d+)?$/.test(text)) return Number(text);
  return text;
}

function writeTab_(name, csv, isReport, description, columnNotes) {
  const spreadsheet = SpreadsheetApp.getActive();
  let sheet = spreadsheet.getSheetByName(name);
  if (!sheet) {
    if (!isReport) throw new Error('there is no tab ' + name);
    sheet = spreadsheet.insertSheet(name);
  }
  const rows = Utilities.parseCsv(csv).filter(r => r.length > 1 || r[0] !== '');
  const width = rows.reduce((w, r) => Math.max(w, r.length), 0);
  const values = rows.map(r => {
    const row = r.map(typed_);
    while (row.length < width) row.push('');
    return row;
  });

  if (sheet.getMaxRows() < values.length + 1) sheet.insertRowsAfter(sheet.getMaxRows(), values.length + 1 - sheet.getMaxRows());
  if (sheet.getMaxColumns() < width) sheet.insertColumnsAfter(sheet.getMaxColumns(), width - sheet.getMaxColumns());

  // Clear only the old values (formatting, checkboxes and dropdowns stay), then write the new ones. A data tab keeps
  // its explanation panel, which sits right of the table (it is rewritten below when Unity sends it).
  const oldWidth = isReport ? sheet.getLastColumn() : headerWidth_(sheet);
  sheet.getRange(1, 1, sheet.getMaxRows(), Math.max(width, oldWidth)).clearContent();
  if (values.length > 0) sheet.getRange(1, 1, values.length, width).setValues(values);
  if (!isReport && columnNotes) writeNotes_(sheet, width, description, columnNotes);
}

// The explanation panel of a data tab: two columns right of the table, after one empty column. The first row says
// what the table is for, then one line per column. Everything right of the table is replaced.
function writeNotes_(sheet, width, description, columnNotes) {
  if (width === 0) return;
  const names = sheet.getRange(1, 1, 1, width).getValues()[0].map(cellText_);
  const rows = [['About this table', description || ''], ['Column', 'What it means']];
  names.forEach((n, i) => rows.push([n, (columnNotes && columnNotes[i]) || '']));

  const start = width + 2;
  if (sheet.getMaxColumns() < start + 1) sheet.insertColumnsAfter(sheet.getMaxColumns(), start + 1 - sheet.getMaxColumns());
  if (sheet.getMaxRows() < rows.length) sheet.insertRowsAfter(sheet.getMaxRows(), rows.length - sheet.getMaxRows());
  // clear() keeps data validation (open-ended checkbox or dropdown columns would reject the text), so drop it too
  const area = sheet.getRange(1, width + 1, sheet.getMaxRows(), sheet.getMaxColumns() - width);
  area.clear();
  area.clearDataValidations();

  const range = sheet.getRange(1, start, rows.length, 2);
  range.setValues(rows).setWrap(true).setVerticalAlignment('top').setFontColor('#434343');
  sheet.getRange(1, start, 1, 2).setBackground('#fff2cc').setFontWeight('bold');
  sheet.getRange(2, start, 1, 2).setBackground('#d9ead3').setFontWeight('bold');
  sheet.getRange(3, start, rows.length - 2, 1).setFontWeight('bold');
  sheet.setColumnWidth(start, 190);
  sheet.setColumnWidth(start + 1, 620);
}
