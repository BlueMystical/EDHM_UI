import assert from 'node:assert/strict';
import { existsSync } from 'node:fs';
import { dirname, join } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

import {
    TP_MOD_DEFAULT_IMAGE_FALLBACK,
    TP_MOD_IMAGE_FALLBACKS,
    TP_MOD_THUMBNAIL_FALLBACKS,
} from '../src/TPMods/TPModImageFallbacks.mjs';

const sourceRoot = join(dirname(fileURLToPath(import.meta.url)), '..', 'src');

test('every current 3PMod has a packaged thumbnail fallback', () => {
    assert.equal(Object.keys(TP_MOD_THUMBNAIL_FALLBACKS).length, 11);

    for (const assetPath of Object.values(TP_MOD_THUMBNAIL_FALLBACKS)) {
        assert.ok(existsSync(join(sourceRoot, assetPath)), `Missing fallback asset: ${assetPath}`);
    }
});

test('every mapped Imgbox fallback is packaged', () => {
    assert.equal(Object.keys(TP_MOD_IMAGE_FALLBACKS).length, 21);

    for (const [remoteUrl, assetPath] of Object.entries(TP_MOD_IMAGE_FALLBACKS)) {
        assert.match(remoteUrl, /^https:\/\/images2\.imgbox\.com\//);
        assert.ok(existsSync(join(sourceRoot, assetPath)), `Missing fallback for ${remoteUrl}: ${assetPath}`);
    }

    assert.ok(existsSync(join(sourceRoot, TP_MOD_DEFAULT_IMAGE_FALLBACK)));
});
