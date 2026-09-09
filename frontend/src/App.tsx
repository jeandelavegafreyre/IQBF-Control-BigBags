import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { useAuth } from './context/AuthContext'
import { LoginPage } from './pages/Login/LoginPage'

function AppHome() {
  const { user, logout } = useAuth()

  return (
    <main className="shell">
      <section className="welcome-panel" aria-labelledby="app-title">
        <span className="eyebrow">Sesión autenticada</span>
        <h1 id="app-title">IQBF Control</h1>
        <p>Sesión iniciada correctamente.</p>

        {user ? (
          <div className="session-summary">
            <p>
              <strong>Usuario:</strong> {user.fullName}
            </p>
            <p>
              <strong>UID:</strong> {user.uid}
            </p>
            <p>
              <strong>Rol:</strong> {user.role}
            </p>
          </div>
        ) : null}

        <button type="button" className="primary-action" onClick={logout}>
          Cerrar sesión
        </button>
      </section>
    </main>
  )
}

export default function App() {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return <div className="session-loading">Cargando sesión…</div>
  }

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <Navigate to="/app" replace /> : <LoginPage />} />
      <Route
        path="/app"
        element={
          <ProtectedRoute>
            <AppHome />
          </ProtectedRoute>
        }
      />
      <Route path="/" element={<Navigate to={isAuthenticated ? '/app' : '/login'} replace />} />
      <Route path="*" element={<Navigate to={isAuthenticated ? '/app' : '/login'} replace />} />
    </Routes>
  )
}
