
import 'bootstrap/dist/js/bootstrap.bundle.min.js'; 
import './index.css'; 
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css'; 

import { createApp } from 'vue';
import App from './MainWindow/App.vue';
import router from './router/index';

const app = createApp(App);
app.use(router);
app.mount('#app');

// Listen for the navigate event from the main process
window.api.navigate((event, route) => {
    router.push(route);
});

//createApp(App).mount('#app');
//console.log('👋 This message is being logged by "renderer.js", included via Vite');


