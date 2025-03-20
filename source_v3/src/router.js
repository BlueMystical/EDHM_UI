import { createRouter, createWebHashHistory } from 'vue-router';
import App from './MainWindow/App.vue';

const routes = [
    { path: '/', name: 'root', redirect: '/App' },
    { path: '/App', name: 'App', component: App },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});

export default router;
