import { createApp, App as VueApp } from 'vue'
import { createPinia, Pinia } from 'pinia'
import { createI18n, I18n } from 'vue-i18n'
import axios, { AxiosInstance } from 'axios'

import '@quasar/extras/roboto-font/roboto-font.css'
import '@quasar/extras/material-icons/material-icons.css'
import 'quasar/src/css/index.sass'

import messages from './i18n/es'

import App from './App.vue'
import router from './router'
import { api } from './boot/api'
import auth from './boot/auth'
import { Quasar, Dialog, Notify, QVueGlobals } from 'quasar'

const i18n: I18n = createI18n({
  legacy: false,
  locale: 'es',
  fallbackLocale: 'es',
  messages: {
    es: messages
  }
})

const app: VueApp = createApp(App)

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