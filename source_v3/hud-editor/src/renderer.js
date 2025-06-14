/**
 * This file will automatically be loaded by vite and run in the "renderer" context.
 * To learn more about the differences between the "main" and the "renderer" context in
 * Electron, visit:
 *
 * https://electronjs.org/docs/tutorial/process-model
 */

import './index.css';
//import 'bootstrap/dist/js/bootstrap.bundle.min.js';  
//import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css'; 

import { createApp } from 'vue';
import App from './app/App.vue';

const app = createApp(App);
app.mount('#app');

console.log('👋 This message is being logged by "renderer.js", included via Vite');
