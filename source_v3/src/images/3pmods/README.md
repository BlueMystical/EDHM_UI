# 3PMods image fallbacks

These files keep the 3PMods manager usable when its primary Imgbox URLs are unavailable.
The application continues to try the server-provided URL first and switches to these packaged files only after an image load error.

- The 11 `*-thumbnail.png` files and seven 500-pixel `*-preview` files were recovered from the EDHM-UI Chromium cache on July 26, 2026.
- `clean-screenshots-preview.jpg`, `hyperspace-preview.jpg`, and `no-external-hud-preview.jpg` came from the corresponding `Resources` directories in `psychicEgg/EDHM` because those three current Imgbox previews were not present in the cache.
- `TPModImageFallbacks.mjs` records the primary URL-to-packaged-file mapping.
