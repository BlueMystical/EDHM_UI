// Compare settings, not names or presentation metadata. The persisted current
// settings already contain the source credits, so no separate last-theme cache
// can become stale when a theme is edited, deleted, or a game instance changes.
function settingsValues(theme) {
  const values = new Map();
  for (const group of theme?.ui_groups || []) {
    for (const element of group.Elements || []) values.set(`hud:${element.Key}`, normalize(element.Value));
  }
  for (const element of theme?.xml_profile || []) values.set(`xml:${element.key}`, normalize(element.value));
  return values;
}

function normalize(value) {
  return typeof value === 'string' && value.trim() !== '' && Number.isFinite(Number(value))
    ? Number(value) : value;
}

function equal(a, b) {
  return a.size > 0 && a.size === b.size && [...a].every(([key, value]) => b.has(key) && b.get(key) === value);
}

export function getCurrentThemeStatus(current, themes, globalSettings, userSettings) {
  const actual = settingsValues(current);
  const sourceName = current?.credits?.theme;
  const namedSource = sourceName && sourceName !== 'Current Settings';
  const source = themes.find(theme => theme.credits?.theme === sourceName);
  // Prefer the applied name when several themes share the same settings, but
  // also recognize a matching Save Copy or a renamed source theme.
  const candidates = source ? [source, ...themes.filter(theme => theme !== source)] : themes;
  for (const theme of candidates) {
    const expected = settingsValues(theme);
    const exact = equal(actual, expected);
    // These overrides are part of Apply Theme. User settings take precedence.
    // Ignore keys outside the template, just as LoadTheme does on reload.
    for (const settings of [globalSettings, userSettings]) {
      for (const element of settings?.Elements || []) {
        const key = `hud:${element.Key}`;
        if (expected.has(key)) expected.set(key, normalize(element.Value));
      }
    }
    if (exact || equal(actual, expected)) {
      return {
        name: `Current Settings: ${theme.credits.theme}`,
        status: exact ? 'Matches saved theme.' : 'Matches saved theme with Global/User overrides.',
      };
    }
  }
  if (namedSource && !source) {
    return {name:'Current Settings: Unverified', status:`Saved theme '${sourceName}' is unavailable; cannot verify a match.`};
  }
  return {
    name:'Current Settings: Custom',
    status: namedSource ? `Changes differ from '${sourceName}'. Save Theme or Save Copy to keep them as a theme.`
      : 'Does not match a saved theme. Save Copy to keep these settings as a theme.',
  };
}
