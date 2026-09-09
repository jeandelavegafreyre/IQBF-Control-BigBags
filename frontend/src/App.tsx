import { useEffect, useState } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { useAuth } from './context/AuthContext'
import { useOperation } from './context/OperationContext'
import { LoginPage } from './pages/Login/LoginPage'
import { OperationsPage } from './pages/Operations/OperationsPage'
import { ShiftStartPage } from './pages/Shift/ShiftStartPage'
import { getActiveShips } from './services/shipService'
import type { Ship } from './types/ships'

function ShipsPage() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const { selectedShip, selectShip, clearOperation } = useOperation()
  const [ships, setShips] = useState<Ship[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    getActiveShips()
      .then((activeShips) => {
        if (!isMounted) return
        setShips(activeShips)
        const availableShip = activeShips.find((ship) => ship.id === selectedShip?.id)
        if (availableShip) selectShip(availableShip)
        else clearOperation()
      })
      .catch(() => {
        if (isMounted) setError('No se pudieron cargar las naves activas.')
      })
      .finally(() => {
        if (isMounted) setIsLoading(false)
      })

    return () => {
      isMounted = false
    }
  }, [clearOperation, selectShip, selectedShip?.id])

  function handleShipSelection(ship: Ship) {
    selectShip(ship)
    navigate('/shift')
  }

  function handleLogout() {
    clearOperation()
    logout()
  }

  return (
    <main className="app-shell">
      <header className="app-header">
        <div>
          <span className="eyebrow">IQBF Control</span>
          <h1 id="app-title">Selecciona una nave</h1>
        </div>
        <button type="button" className="secondary-action" onClick={handleLogout}>
          Cerrar sesión
        </button>
      </header>

      <section className="ship-selection" aria-labelledby="ship-selection-title">
        <div className="section-heading">
          <div>
            <span className="eyebrow">Operación</span>
            <h2 id="ship-selection-title">Naves activas</h2>
          </div>
          {user ? <span className="user-badge">{user.fullName}</span> : null}
        </div>

        {isLoading ? <p className="status-message">Cargando naves activas...</p> : null}
        {error ? <p className="status-message error-message" role="alert">{error}</p> : null}
        {!isLoading && !error && ships.length === 0 ? (
          <p className="status-message">No hay naves activas disponibles.</p>
        ) : null}

        {!isLoading && !error && ships.length > 0 ? (
          <div className="ship-grid">
            {ships.map((ship) => {
              const isSelected = selectedShip?.id === ship.id
              return (
                <button
                  type="button"
                  className={`ship-option${isSelected ? ' is-selected' : ''}`}
                  key={ship.id}
                  aria-pressed={isSelected}
                  onClick={() => handleShipSelection(ship)}
                >
                  <span className="ship-status">Activa</span>
                  <strong>{ship.name}</strong>
                  <span>{isSelected ? 'Nave seleccionada' : 'Seleccionar nave'}</span>
                </button>
              )
            })}
          </div>
        ) : null}

      </section>
    </main>
  )
}

function ShiftRoute() {
  const { selectedShip } = useOperation()

  return selectedShip ? <ShiftStartPage /> : <Navigate to="/ships" replace />
}

function OperationsRoute() {
  const { selectedShip, activeShift } = useOperation()

  if (!selectedShip) {
    return <Navigate to="/ships" replace />
  }

  if (!activeShift) {
    return <Navigate to="/shift" replace />
  }

  return <OperationsPage />
}

export default function App() {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return <div className="session-loading">Cargando sesión…</div>
  }

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <Navigate to="/ships" replace /> : <LoginPage />} />
      <Route
        path="/ships"
        element={
          <ProtectedRoute>
            <ShipsPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/shift"
        element={
          <ProtectedRoute>
            <ShiftRoute />
          </ProtectedRoute>
        }
      />
      <Route
        path="/operations"
        element={
          <ProtectedRoute>
            <OperationsRoute />
          </ProtectedRoute>
        }
      />
      <Route path="/app" element={<Navigate to="/ships" replace />} />
      <Route path="/" element={<Navigate to={isAuthenticated ? '/ships' : '/login'} replace />} />
      <Route path="*" element={<Navigate to={isAuthenticated ? '/ships' : '/login'} replace />} />
    </Routes>
  )
}
