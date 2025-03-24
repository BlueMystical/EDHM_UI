import { createRouter, createWebHashHistory } from 'vue-router';
import TPModsManager from '../TPMods/TPModsManager.vue';
import MainComponent from '../MainWindow/App.vue'; // Asegúrate de que la ruta sea correcta

const routes = [
  { path: '/', component: MainComponent }, // Ruta principal
  { path: '/tp-mods-manager', component: TPModsManager },
];

const router = createRouter({
  history: createWebHashHistory(),
  routes,
});

export default router;