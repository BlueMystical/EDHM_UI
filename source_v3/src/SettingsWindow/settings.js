import { createApp } from 'vue'
import SettingsEditor from './SettingsEditor.vue'

const app = createApp(SettingsEditor)
app.mount('#app')

// Recibir datos desde main
window.api?.settings.onInit((data) => {
  // Pasar props dinámicamente
  app._instance.props = { ...app._instance.props, initData: data }
})