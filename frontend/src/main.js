import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createI18n } from 'vue-i18n'
import axios from 'axios'

import '@quasar/extras/roboto-font/roboto-font.css'
import '@quasar/extras/material-icons/material-icons.css'
import 'quasar/src/css/index.sass'

import messages from './locales/es'

import App from './App.vue'
import router from './router'
import { api } from './boot/api'
import auth from './boot/auth'
import { Quasar, Dialog, Notify } from 'quasar'

const i18n = createI18n({
  legacy: false,
  locale: 'es',
  fallbackLocale: 'es',
  messages: {
    es: messages
  }
})

const app = createApp(App)

app.config.globalProperties.$axios = axios
app.config.globalProperties.$api = api

app.use(createPinia())
app.use(Quasar, {
  plugins: {
    Dialog,
    Notify
  }
})
app.use(router)
app.use(i18n)
app.use(auth)

app.mount('#q-app')