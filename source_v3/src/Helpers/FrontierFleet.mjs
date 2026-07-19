export const normalizeShipSymbol = (value) => String(value || '')
  .trim()
  .replace(/^\$shipname_/i, '')
  .replace(/_name;?$/i, '')
  .replace(/;$/g, '')
  .toLowerCase()
  .replace(/[^a-z0-9]/g, '');

export const normalizeOwnedShipId = (value) => {
  if (value === null || value === undefined) return null;
  const normalized = String(value).trim();
  return normalized === '' ? null : normalized;
};

const normalizeIdentityText = (value) => String(value ?? '').trim().toLowerCase();

const getShipName = (ship) => {
  if (typeof ship?.name === 'string') return ship.name;
  if (typeof ship?.custom_name === 'string') return ship.custom_name;
  return '';
};

const hasSameModel = (left, right) =>
  normalizeShipSymbol(left?.kind_short) === normalizeShipSymbol(right?.kind_short);

const hasSameLegacyIdentity = (left, right) =>
  hasSameModel(left, right) &&
  normalizeIdentityText(getShipName(left)) === normalizeIdentityText(getShipName(right)) &&
  normalizeIdentityText(left?.plate) === normalizeIdentityText(right?.plate);

const pickCustomization = (frontierShip, candidates) => {
  const themed = candidates.find((ship) =>
    ship?.theme && ship.theme !== 'Current Settings'
  ) || candidates.find((ship) => ship?.theme);
  const customImage = candidates.find((ship) =>
    ship?.image && ship.image !== frontierShip.image
  ) || candidates.find((ship) => ship?.image);

  return {
    theme: themed?.theme || frontierShip.theme,
    image: customImage?.image || frontierShip.image,
  };
};

export const normalizeFrontierShips = (profile, shipListData = []) => {
  const shipsValue = profile?.ships;
  const entries = Array.isArray(shipsValue)
    ? shipsValue.map((ship, index) => [String(ship?.id ?? index), ship])
    : Object.entries(shipsValue || {});

  if (profile?.ship) {
    const currentId = String(profile.ship.id ?? profile.commander?.currentShipId ?? 'current');
    if (!entries.some(([key, ship]) => String(ship?.id ?? key) === currentId)) {
      entries.push([currentId, profile.ship]);
    }
  }

  return entries
    .filter(([, ship]) => ship && typeof ship === 'object')
    .map(([entryId, ship]) => {
      const frontierId = normalizeOwnedShipId(ship.id ?? entryId);
      const symbol = ship.name || ship.shipType || ship.ship;
      const normalizedSymbol = normalizeShipSymbol(symbol);
      const model = shipListData.find((candidate) =>
        normalizeShipSymbol(candidate.ed_short) === normalizedSymbol ||
        normalizeShipSymbol(candidate.ship_full_name) === normalizedSymbol
      );
      const kindShort = model?.ed_short?.toLowerCase() || String(symbol || 'unknown').toLowerCase();
      const customName = typeof ship.shipName === 'string'
        ? ship.shipName.trim()
        : String(model?.ship_full_name || symbol || 'Unnamed Ship').trim();
      return {
        record_id: `frontier:${frontierId}`,
        frontier_id: frontierId,
        ship_id: model?.ship_id ?? `frontier-${frontierId}`,
        kind_short: kindShort,
        kind_full: model?.ship_full_name || String(symbol || 'Unknown Ship'),
        name: customName,
        plate: String(ship.shipID || '').trim(),
        theme: 'Current Settings',
        image: model ? `${model.ed_short.toLowerCase()}.jpg` : '@default.jpg',
        source: 'frontier',
      };
    });
};

export const mergeFrontierShips = (frontierShips, existingShips = []) => {
  const claimedIndexes = new Set();
  const customizationCandidates = frontierShips.map(() => []);

  // First correlate by the game-owned numeric ID. This remains reliable when
  // the Commander renames a ship or changes its displayed identifier.
  frontierShips.forEach((frontierShip, frontierIndex) => {
    const frontierId = normalizeOwnedShipId(frontierShip.frontier_id);
    if (!frontierId) return;

    existingShips.forEach((existingShip, existingIndex) => {
      if (claimedIndexes.has(existingIndex)) return;
      if (normalizeOwnedShipId(existingShip.frontier_id) !== frontierId) return;

      claimedIndexes.add(existingIndex);
      // Frontier can reuse an ID after a ship is sold. Consume the stale
      // record, but do not carry its customization to a different model.
      if (hasSameModel(existingShip, frontierShip)) {
        customizationCandidates[frontierIndex].push(existingShip);
      }
    });
  });

  // Migrate one legacy Journal/V2 record per otherwise-unmatched Frontier
  // ship. Records with another stable ID must never be collapsed by name.
  frontierShips.forEach((frontierShip, frontierIndex) => {
    if (customizationCandidates[frontierIndex].length > 0) return;
    const legacyIndex = existingShips.findIndex((existingShip, existingIndex) =>
      !claimedIndexes.has(existingIndex) &&
      !normalizeOwnedShipId(existingShip.frontier_id) &&
      hasSameLegacyIdentity(existingShip, frontierShip)
    );
    if (legacyIndex < 0) return;

    claimedIndexes.add(legacyIndex);
    customizationCandidates[frontierIndex].push(existingShips[legacyIndex]);
  });

  // Remove leftover no-ID records that describe a ship already represented by
  // Frontier. These are duplicates from older journal/import behavior. Attach
  // any useful customization to one matching Frontier record before removal.
  existingShips.forEach((existingShip, existingIndex) => {
    if (claimedIndexes.has(existingIndex)) return;
    if (normalizeOwnedShipId(existingShip.frontier_id)) return;

    const frontierIndex = frontierShips.findIndex((frontierShip) =>
      hasSameLegacyIdentity(existingShip, frontierShip)
    );
    if (frontierIndex < 0) return;

    claimedIndexes.add(existingIndex);
    customizationCandidates[frontierIndex].push(existingShip);
  });

  const mergedFrontierShips = frontierShips.map((frontierShip, frontierIndex) => {
    const customization = pickCustomization(
      frontierShip,
      customizationCandidates[frontierIndex]
    );
    return {
      ...frontierShip,
      ...customization,
    };
  });

  const localOnlyShips = existingShips.filter((ship, index) =>
    !claimedIndexes.has(index) && ship.source !== 'frontier'
  );
  return [...mergedFrontierShips, ...localOnlyShips];
};

export const upsertJournalShip = (journalShip, existingShips = []) => {
  const frontierId = normalizeOwnedShipId(journalShip?.frontier_id);
  const incoming = {
    ...journalShip,
    ...(frontierId ? { frontier_id: frontierId } : {}),
    kind_short: String(journalShip?.kind_short || '').trim().toLowerCase(),
    name: String(journalShip?.name ?? '').trim(),
    plate: String(journalShip?.plate ?? '').trim(),
  };

  let existingIndex = -1;
  if (frontierId) {
    existingIndex = existingShips.findIndex((ship) =>
      normalizeOwnedShipId(ship.frontier_id) === frontierId
    );
  }

  if (existingIndex < 0) {
    existingIndex = existingShips.findIndex((ship) =>
      // When the journal supplies a stable ID, only migrate an ID-less legacy
      // record. Never collapse another owned ship that has a different ID.
      (!frontierId || !normalizeOwnedShipId(ship.frontier_id)) &&
      hasSameLegacyIdentity(ship, incoming)
    );
  }

  if (existingIndex < 0) {
    const addedShip = {
      ...incoming,
      ...(frontierId ? { record_id: `journal:${frontierId}` } : {}),
      source: 'journal',
    };
    return {
      ships: [...existingShips, addedShip],
      ship: addedShip,
      changed: true,
      added: true,
    };
  }

  const existing = existingShips[existingIndex];
  const sameModel = hasSameModel(existing, incoming);
  const { custom_name: _legacyName, ...existingWithoutLegacyName } = existing;
  const updatedShip = sameModel
    ? {
        ...existingWithoutLegacyName,
        ...incoming,
        record_id: existing.record_id || (frontierId ? `journal:${frontierId}` : undefined),
        frontier_id: frontierId || normalizeOwnedShipId(existing.frontier_id) || undefined,
        source: existing.source || 'journal',
        theme: existing.theme || incoming.theme,
        image: existing.image || incoming.image,
      }
    : {
        ...incoming,
        ...(frontierId ? {
          record_id: `journal:${frontierId}`,
          frontier_id: frontierId,
        } : {}),
        source: 'journal',
      };

  const ships = [...existingShips];
  ships[existingIndex] = updatedShip;
  return {
    ships,
    ship: updatedShip,
    changed: JSON.stringify(existing) !== JSON.stringify(updatedShip),
    added: false,
  };
};
