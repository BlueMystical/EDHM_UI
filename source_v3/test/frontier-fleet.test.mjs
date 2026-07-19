import test from 'node:test';
import assert from 'node:assert/strict';
import {
  mergeFrontierShips,
  normalizeFrontierShips,
  normalizeShipSymbol,
  upsertJournalShip,
} from '../src/Helpers/FrontierFleet.mjs';

const shipList = [
  { ship_id: 9, ed_short: 'cobramkiii', ship_full_name: 'Cobra Mk 3' },
  { ship_id: 35, ed_short: 'python', ship_full_name: 'Python' },
];

test('normalizes Frontier symbols and ID-keyed fleet responses', () => {
  assert.equal(normalizeShipSymbol('$ShipName_CobraMkIII_Name;'), 'cobramkiii');
  const ships = normalizeFrontierShips({
    commander: { currentShipId: 42 },
    ships: {
      42: { id: 42, name: 'CobraMkIII', shipName: 'Raven', shipID: 'RVN-01' },
      77: { id: 77, name: 'Python', shipName: 'Hauler', shipID: 'PY-77' },
    },
    ship: { id: 42, name: 'CobraMkIII', shipName: 'Raven', shipID: 'RVN-01' },
  }, shipList);

  assert.equal(ships.length, 2, 'current ship is not duplicated');
  assert.deepEqual(ships[0], {
    record_id: 'frontier:42',
    frontier_id: '42',
    ship_id: 9,
    kind_short: 'cobramkiii',
    kind_full: 'Cobra Mk 3',
    name: 'Raven',
    plate: 'RVN-01',
    theme: 'Current Settings',
    image: 'cobramkiii.jpg',
    source: 'frontier',
  });
});

test('supports array fleets and unknown ship models', () => {
  const ships = normalizeFrontierShips({
    ships: [{ id: 101, name: 'FutureShip', shipName: 'Prototype', shipID: 'NEW-01' }],
  }, shipList);

  assert.equal(ships[0].frontier_id, '101');
  assert.equal(ships[0].kind_full, 'FutureShip');
  assert.equal(ships[0].image, '@default.jpg');
});

test('preserves an intentionally blank custom ship name', () => {
  const ships = normalizeFrontierShips({
    ships: [{ id: 42, name: 'CobraMkIII', shipName: '', shipID: 'RVN-01' }],
  }, shipList);

  assert.equal(ships[0].name, '');
  assert.equal(ships[0].kind_full, 'Cobra Mk 3');
});

test('fleet refresh preserves customizations and local-only records', () => {
  const frontierShips = normalizeFrontierShips({
    ships: [{ id: 42, name: 'CobraMkIII', shipName: 'Raven', shipID: 'RVN-01' }],
  }, shipList);
  const existingShips = [
    {
      frontier_id: '42', source: 'frontier', kind_short: 'cobramkiii', name: 'Raven', plate: 'RVN-01',
      theme: 'Blue Theme', image: 'custom-raven.jpg',
    },
    {
      frontier_id: '88', source: 'frontier', kind_short: 'python', name: 'Sold Ship', plate: 'OLD-88',
      theme: 'Current Settings', image: 'python.jpg',
    },
    {
      record_id: 'journal:srv', source: 'journal', kind_short: 'testbuggy', name: 'Scarab', plate: 'SRV_1',
      theme: 'SRV Theme', image: 'testbuggy.jpg',
    },
  ];

  const merged = mergeFrontierShips(frontierShips, existingShips);
  assert.equal(merged.length, 2, 'sold Frontier records are removed while local records remain');
  assert.equal(merged[0].theme, 'Blue Theme');
  assert.equal(merged[0].image, 'custom-raven.jpg');
  assert.equal(merged[1].record_id, 'journal:srv');
});

test('migrates legacy custom_name records without creating Frontier duplicates', () => {
  const frontierShips = normalizeFrontierShips({
    ships: [{ id: 23, name: 'Cutter', shipName: '', shipID: 'FE-17C' }],
  }, [
    ...shipList,
    { ship_id: 23, ed_short: 'cutter', ship_full_name: 'Imperial Cutter' },
  ]);
  const existingShips = [{
    ship_id: 23,
    kind_short: 'cutter',
    kind_full: 'Imperial Cutter',
    custom_name: ' ',
    plate: 'fe-17c ',
    theme: 'Legacy Orange',
    image: 'custom-cutter.jpg',
  }];

  const merged = mergeFrontierShips(frontierShips, existingShips);
  assert.equal(merged.length, 1);
  assert.equal(merged[0].frontier_id, '23');
  assert.equal(merged[0].name, '');
  assert.equal(merged[0].theme, 'Legacy Orange');
  assert.equal(merged[0].image, 'custom-cutter.jpg');
  assert.equal('custom_name' in merged[0], false);
});

test('keeps same-model ships separate by Frontier ID even with identical names and plates', () => {
  const frontierShips = normalizeFrontierShips({
    ships: [
      { id: 42, name: 'CobraMkIII', shipName: 'Raven', shipID: 'RVN-01' },
      { id: 43, name: 'CobraMkIII', shipName: 'Raven', shipID: 'RVN-01' },
    ],
  }, shipList);
  const existingShips = [
    { frontier_id: '42', source: 'frontier', kind_short: 'cobramkiii', name: 'Raven', plate: 'RVN-01', theme: 'Blue' },
    { frontier_id: '43', source: 'frontier', kind_short: 'cobramkiii', name: 'Raven', plate: 'RVN-01', theme: 'Green' },
  ];

  const merged = mergeFrontierShips(frontierShips, existingShips);
  assert.equal(merged.length, 2);
  assert.deepEqual(merged.map((ship) => [ship.frontier_id, ship.theme]), [
    ['42', 'Blue'],
    ['43', 'Green'],
  ]);
});

test('removes an existing legacy duplicate while preserving its assigned theme', () => {
  const frontierShips = normalizeFrontierShips({
    ships: [{ id: 42, name: 'CobraMkIII', shipName: 'Raven', shipID: 'RVN-01' }],
  }, shipList);
  const existingShips = [
    {
      frontier_id: '42', source: 'frontier', kind_short: 'cobramkiii',
      name: 'Raven', plate: 'RVN-01', theme: 'Current Settings', image: 'cobramkiii.jpg',
    },
    {
      kind_short: 'CobraMkIII', custom_name: ' raven ', plate: 'rvn-01',
      theme: 'Legacy Blue', image: 'custom-raven.jpg',
    },
  ];

  const merged = mergeFrontierShips(frontierShips, existingShips);
  assert.equal(merged.length, 1);
  assert.equal(merged[0].frontier_id, '42');
  assert.equal(merged[0].theme, 'Legacy Blue');
  assert.equal(merged[0].image, 'custom-raven.jpg');
});

test('journal rename matches by stable ID and preserves the assigned theme', () => {
  const existingShips = [{
    record_id: 'frontier:42',
    frontier_id: '42',
    source: 'frontier',
    kind_short: 'cobramkiii',
    kind_full: 'Cobra Mk 3',
    name: 'Old Name',
    plate: 'OLD-01',
    theme: 'Blue Theme',
    image: 'custom-raven.jpg',
  }];

  const result = upsertJournalShip({
    frontier_id: 42,
    ship_id: 9,
    kind_short: 'CobraMkIII',
    kind_full: 'Cobra Mk 3',
    name: 'New Name',
    plate: 'NEW-01',
    theme: 'Current Settings',
    image: 'cobramkiii.jpg',
  }, existingShips);

  assert.equal(result.ships.length, 1);
  assert.equal(result.ship.record_id, 'frontier:42');
  assert.equal(result.ship.name, 'New Name');
  assert.equal(result.ship.plate, 'NEW-01');
  assert.equal(result.ship.theme, 'Blue Theme');
  assert.equal(result.ship.image, 'custom-raven.jpg');

  const repeated = upsertJournalShip(result.ship, result.ships);
  assert.equal(repeated.changed, false, 'repeating the same journal identity is idempotent');
  assert.equal(repeated.ships.length, 1);
});

test('journal matching migrates legacy names and does not collapse another stable ID', () => {
  const legacyResult = upsertJournalShip({
    frontier_id: 42,
    ship_id: 9,
    kind_short: 'cobramkiii',
    kind_full: 'Cobra Mk 3',
    name: ' Raven ',
    plate: 'rvn-01',
    theme: 'Current Settings',
    image: 'cobramkiii.jpg',
  }, [{
    kind_short: 'CobraMkIII', custom_name: 'raven', plate: 'RVN-01',
    theme: 'Legacy Theme', image: 'legacy.jpg',
  }]);

  assert.equal(legacyResult.ships.length, 1);
  assert.equal(legacyResult.ship.frontier_id, '42');
  assert.equal(legacyResult.ship.theme, 'Legacy Theme');
  assert.equal('custom_name' in legacyResult.ship, false);

  const separateResult = upsertJournalShip({
    frontier_id: 43,
    ship_id: 9,
    kind_short: 'cobramkiii',
    kind_full: 'Cobra Mk 3',
    name: 'Raven',
    plate: 'RVN-01',
    theme: 'Current Settings',
    image: 'cobramkiii.jpg',
  }, legacyResult.ships);

  assert.equal(separateResult.ships.length, 2);
  assert.deepEqual(separateResult.ships.map((ship) => ship.frontier_id), ['42', '43']);
});

test('a reused Frontier ID on a different model does not inherit the sold ship theme', () => {
  const frontierShips = normalizeFrontierShips({
    ships: [{ id: 42, name: 'Python', shipName: 'Replacement', shipID: 'NEW-42' }],
  }, shipList);
  const merged = mergeFrontierShips(frontierShips, [{
    frontier_id: '42', source: 'frontier', kind_short: 'cobramkiii',
    name: 'Sold Ship', plate: 'OLD-42', theme: 'Old Blue', image: 'old.jpg',
  }]);

  assert.equal(merged.length, 1);
  assert.equal(merged[0].kind_short, 'python');
  assert.equal(merged[0].theme, 'Current Settings');
  assert.equal(merged[0].image, 'python.jpg');
});
