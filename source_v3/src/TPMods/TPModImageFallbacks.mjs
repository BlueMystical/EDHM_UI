const TP_MOD_IMAGE_ROOT = 'images/3pmods';

export const TP_MOD_DEFAULT_IMAGE_FALLBACK = 'images/3PM_Default.png';

export const TP_MOD_THUMBNAIL_FALLBACKS = Object.freeze({
    'Clean Screenshots Mod': `${TP_MOD_IMAGE_ROOT}/clean-screenshots-thumbnail.png`,
    'ED-GT [Graphic Tweaks]': `${TP_MOD_IMAGE_ROOT}/ed-gt-thumbnail.png`,
    'FSS-Overlay': `${TP_MOD_IMAGE_ROOT}/fss-overlay-thumbnail.png`,
    'Fuel Scooping Stars Mod': `${TP_MOD_IMAGE_ROOT}/fuel-scooping-stars-thumbnail.png`,
    'Hyperspace Mod': `${TP_MOD_IMAGE_ROOT}/hyperspace-thumbnail.png`,
    'No External HUD Mod': `${TP_MOD_IMAGE_ROOT}/no-external-hud-thumbnail.png`,
    'No FID Mod': `${TP_MOD_IMAGE_ROOT}/no-fid-thumbnail.png`,
    'No Message Box Mod': `${TP_MOD_IMAGE_ROOT}/no-message-box-thumbnail.png`,
    'Odyssey Key Bindings': `${TP_MOD_IMAGE_ROOT}/odyssey-key-bindings-thumbnail.png`,
    'Odyssey Light Night Mod': `${TP_MOD_IMAGE_ROOT}/odyssey-light-night-thumbnail.png`,
    'Thick Orbit Lines Mod': `${TP_MOD_IMAGE_ROOT}/thick-orbit-lines-thumbnail.png`,
});

// The server list remains the primary source. These mappings are used only
// after Chromium reports that an Imgbox image failed to load.
export const TP_MOD_IMAGE_FALLBACKS = Object.freeze({
    'https://images2.imgbox.com/ac/e4/UVU0fONO_o.png': `${TP_MOD_IMAGE_ROOT}/clean-screenshots-thumbnail.png`,
    'https://images2.imgbox.com/df/dd/C5ceWcFm_o.jpg': `${TP_MOD_IMAGE_ROOT}/clean-screenshots-preview.jpg`,
    'https://images2.imgbox.com/5f/22/3oM3eTcl_o.png': `${TP_MOD_IMAGE_ROOT}/ed-gt-thumbnail.png`,
    'https://images2.imgbox.com/80/d6/KM2miiuh_o.jpg': `${TP_MOD_IMAGE_ROOT}/ed-gt-preview.jpg`,
    'https://images2.imgbox.com/7b/e4/o3hdmBNF_o.png': `${TP_MOD_IMAGE_ROOT}/fss-overlay-thumbnail.png`,
    'https://images2.imgbox.com/76/5e/MwFm92xw_o.jpg': `${TP_MOD_IMAGE_ROOT}/fss-overlay-preview.jpg`,
    'https://images2.imgbox.com/4a/63/QfE24L0u_o.png': `${TP_MOD_IMAGE_ROOT}/fuel-scooping-stars-thumbnail.png`,
    'https://images2.imgbox.com/c6/8a/0RKcSksZ_o.jpg': `${TP_MOD_IMAGE_ROOT}/fuel-scooping-stars-preview.jpg`,
    'https://images2.imgbox.com/26/3d/lHxhDb6q_o.png': `${TP_MOD_IMAGE_ROOT}/hyperspace-thumbnail.png`,
    'https://images2.imgbox.com/6e/12/EdbI6yS8_o.jpg': `${TP_MOD_IMAGE_ROOT}/hyperspace-preview.jpg`,
    'https://images2.imgbox.com/28/bb/OsKWaTJo_o.png': `${TP_MOD_IMAGE_ROOT}/no-external-hud-thumbnail.png`,
    'https://images2.imgbox.com/f1/62/6nj4qVWf_o.jpg': `${TP_MOD_IMAGE_ROOT}/no-external-hud-preview.jpg`,
    'https://images2.imgbox.com/83/cf/E9W7huJo_o.png': `${TP_MOD_IMAGE_ROOT}/no-fid-thumbnail.png`,
    'https://images2.imgbox.com/86/f3/sTuWqf7E_o.jpg': `${TP_MOD_IMAGE_ROOT}/no-fid-preview.jpg`,
    'https://images2.imgbox.com/fb/79/dGJBIBTP_o.png': `${TP_MOD_IMAGE_ROOT}/no-message-box-thumbnail.png`,
    'https://images2.imgbox.com/aa/b8/2ewFeCdC_o.jpg': `${TP_MOD_IMAGE_ROOT}/no-message-box-preview.jpg`,
    'https://images2.imgbox.com/85/75/3tRO56WS_o.png': `${TP_MOD_IMAGE_ROOT}/odyssey-key-bindings-thumbnail.png`,
    'https://images2.imgbox.com/c7/72/AMeMgguV_o.png': `${TP_MOD_IMAGE_ROOT}/odyssey-light-night-thumbnail.png`,
    'https://images2.imgbox.com/c5/7f/8FUU9nev_o.jpg': `${TP_MOD_IMAGE_ROOT}/odyssey-light-night-preview.jpg`,
    'https://images2.imgbox.com/4a/99/qz8V04Qw_o.png': `${TP_MOD_IMAGE_ROOT}/thick-orbit-lines-thumbnail.png`,
    'https://images2.imgbox.com/be/55/mwUzvoLa_o.png': `${TP_MOD_IMAGE_ROOT}/thick-orbit-lines-preview.png`,
});
