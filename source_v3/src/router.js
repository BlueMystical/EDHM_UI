import { createRouter, createWebHashHistory } from 'vue-router';
import App from './MainWindow/App.vue';
//import TPModsManager from './TPMods/TPModsManager.vue';

const routes = [
    { path: '/', redirect: '/App' }, // Redirect root route
    { path: '/App', component: App }, // Assign App.vue to its own route
    { path: '/TPModsManager', 
        //component: TPModsManager 
        component: () => require('./TPMods/TPModsManager.vue').default
    }, // Route for TPModsManager.vue
  ];
  

const router = createRouter({
  history: createWebHashHistory(),
  routes,
});

//console.log('Routes:', router.getRoutes());

export default router;

