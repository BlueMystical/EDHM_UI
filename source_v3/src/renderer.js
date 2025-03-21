
import 'bootstrap/dist/js/bootstrap.bundle.min.js'; 
import './index.css'; 
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css'; 

import { createApp } from 'vue';
import App from './MainWindow/App.vue';
//import router from './router'; // Import the router

const app = createApp(App);
//app.use(router); // Use the router
app.mount('#app');


//createApp(App).mount('#app');
//console.log('👋 This message is being logged by "renderer.js", included via Vite');


