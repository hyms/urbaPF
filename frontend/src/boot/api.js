import { boot } from 'quasar/wrappers'

const api = boot(($axios) => {
  const baseURL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

  $axios.setBaseURL(baseURL)

  $axios.interceptors.request.use((config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })

  $axios.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response?.status === 401) {
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        window.location.href = '/login'
      }
      return Promise.reject(error)
    }
  )
})

export default api

export { api }
