import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { Quasar, Dialog, Notify } from 'quasar'

import '@quasar/extras/roboto-font/roboto-font.css'
import '@quasar/extras/material-icons/material-icons.css'
import 'quasar/src/css/index.sass'

import App from './App.vue'
import router from './router'
import api from './boot/api'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(api)
app.use(Quasar, {
  plugins: {
    Dialog,
    Notify
  }
})

app.mount('#q-app')
