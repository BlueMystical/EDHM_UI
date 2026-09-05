import fs from 'node:fs';
import path from 'node:path';

// Use the same archive and version for installation and shared app metadata.
export function getModBundle(assetsPath, gameType) {
  if (!['ODYSS', 'HORIZ'].includes(gameType)) {
    throw new Error(`Unknown EDHM game type: ${gameType}`);
  }
  const pattern = new RegExp(`^${gameType}_EDHM-(v\\d+(?:\\.\\d+)*(?:\\.?[a-z]+)?)\\.zip$`, 'i');
  const bundles = fs.readdirSync(assetsPath, { withFileTypes: true })
    .filter(entry => entry.isFile() && pattern.test(entry.name));
  if (bundles.length !== 1) {
    throw new Error(`Expected one ${gameType} EDHM bundle in ${assetsPath}; found ${bundles.length}`);
  }
  return {
    filePath: path.join(assetsPath, bundles[0].name),
    version: bundles[0].name.match(pattern)[1],
  };
}
