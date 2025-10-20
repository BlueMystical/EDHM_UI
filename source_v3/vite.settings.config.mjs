// vite.settings.config.mjs
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import path from 'path';

export default defineConfig({
  root: path.resolve(__dirname, 'src/SettingsWindow'),
  base: './',
  build: {
    rollupOptions: {
      input: path.resolve(__dirname, 'src/SettingsWindow/settings.html'),
    },
    outDir: path.resolve(__dirname, 'out/renderer/settings_window'),
    emptyOutDir: true,
  },
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});