import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { Quasar, Dialog, Notify } from 'quasar'
import axios from 'axios'

import '@quasar/extras/roboto-font/roboto-font.css'
import '@quasar/extras/material-icons/material-icons.css'
import 'quasar/src/css/index.sass'

import App from './App.vue'
import router from './router'
import { api } from './boot/api'
import auth from './boot/auth'

const app = createApp(App)

app.config.globalProperties.$axios = axios
app.config.globalProperties.$api = api

app.use(createPinia())
app.use(router)
app.use(auth)
app.use(Quasar, {
  plugins: {
    Dialog,
    Notify
  }
})

app.mount('#q-app')
