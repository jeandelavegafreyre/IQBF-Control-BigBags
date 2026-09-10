import axios from 'axios'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status
    const hadSession = Boolean(localStorage.getItem('token'))
    const requestUrl = String(error?.config?.url ?? '')
    const isLoginRequest = requestUrl.includes('/api/auth/login')

    // Un 401 durante una sesión autenticada significa que el JWT expiró,
    // el usuario fue desactivado o sus permisos cambiaron. Limpiamos la
    // sesión local y regresamos al login una sola vez.
    if (status === 401 && hadSession && !isLoginRequest) {
      localStorage.removeItem('token')
      localStorage.removeItem('authUser')
      sessionStorage.setItem(
        'sessionMessage',
        'Tu sesión ya no es válida. Inicia sesión nuevamente.',
      )

      if (window.location.pathname !== '/login') {
        window.location.replace('/login')
      }
    }

    return Promise.reject(error)
  },
)

export default apiClient
