import { type FormEvent, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import './LoginPage.css'

export function LoginPage() {
  const navigate = useNavigate()
  const { login, isAuthenticated, isLoading } = useAuth()
  const [uid, setUid] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate('/ships', { replace: true })
    }
  }, [isAuthenticated, isLoading, navigate])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')

    const trimmedUid = uid.trim()

    if (!trimmedUid || !password.trim()) {
      setError('Debe introducir el UID y la contraseña.')
      return
    }

    setIsSubmitting(true)

    try {
      await login({ uid: trimmedUid, password })
      navigate('/ships', { replace: true })
    } catch (caughtError) {
      const httpError = caughtError as {
        response?: { data?: { message?: string; error?: string } }
        message?: string
      }

      const backendMessage =
        httpError?.response?.data?.message ||
        httpError?.response?.data?.error ||
        httpError?.message ||
        'No se pudo iniciar sesión. Revise sus credenciales.'

      setError(backendMessage)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-page">
      <div className="login-shell">
        <div className="login-card">
          <div className="login-header">
            <span className="brand-badge">IQBF</span>
            <h1>IQBF Control</h1>
            <p>Recepción vs Despacho - Big Bags</p>
          </div>

          <form className="login-form" onSubmit={handleSubmit} noValidate>
            <label className="login-field">
              <span>UID</span>
              <input
                type="text"
                name="uid"
                autoComplete="username"
                value={uid}
                onChange={(event) => setUid(event.target.value)}
                placeholder="Ingrese su UID"
                disabled={isSubmitting}
              />
            </label>

            <label className="login-field">
              <span>Contraseña</span>
              <input
                type="password"
                name="password"
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Ingrese su contraseña"
                disabled={isSubmitting}
              />
            </label>

            {error ? <div className="login-error">{error}</div> : null}

            <button type="submit" className="login-button" disabled={isSubmitting}>
              {isSubmitting ? 'Iniciando sesión...' : 'Iniciar sesión'}
            </button>
          </form>
        </div>
      </div>
    </main>
  )
}
