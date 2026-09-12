import { useEffect, useState } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { useAuth } from './context/AuthContext'
import { useOperation } from './context/OperationContext'
import { LoginPage } from './pages/Login/LoginPage'
import { OperationsPage } from './pages/Operations/OperationsPage'
import './pages/Operations/OperationsRefinements.css'
import { HistoryPage } from './pages/History/HistoryPage'
import { ReportsPage } from './pages/Reports/ReportsPage'
import { ManagementReportsPage } from './pages/Management/ManagementReportsPage'
import { AdminPage } from './pages/Admin/AdminPage'
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
    <main className="app-shell ship-page-shell">
      <header className="app-header ship-page-header">
        <div>
          <span className="eyebrow">IQBF Control</span>
          <h1 id="app-title">Selecciona una nave</h1>
          <p className="ship-page-subtitle">Elige la nave que vas a gestionar para continuar con la selección del turno.</p>
        </div>
        <div className="operations-header-actions">
          {user?.role === 'Administrator' ? <button type="button" className="secondary-action" onClick={() => navigate('/admin')}>Configuración</button> : null}
          {user?.role === 'Administrator' ? <button type="button" className="secondary-action" onClick={() => navigate('/management')}>Panel Gerencial</button> : null}
          <button type="button" className="secondary-action" onClick={handleLogout}>Cerrar sesión</button>
        </div>
      </header>

      <section className="ship-selection ship-selection-panel" aria-labelledby="ship-selection-title">
        <div className="ship-selection-topbar">
          <div className="section-heading-copy">
            <span className="eyebrow">Operación</span>
            <h2 id="ship-selection-title">Naves disponibles</h2>
            <p>Solo se muestran las naves activadas por el Administrador.</p>
          </div>
          <div className="ship-selection-meta">
            <span className="ship-count-badge">{ships.length} {ships.length === 1 ? 'nave activa' : 'naves activas'}</span>
            {user ? <span className="user-badge">{user.fullName}</span> : null}
          </div>
        </div>

        {isLoading ? <p className="status-message ship-empty-state">Cargando naves activas...</p> : null}
        {error ? <p className="status-message error-message ship-empty-state" role="alert">{error}</p> : null}
        {!isLoading && !error && ships.length === 0 ? (
          <div className="ship-empty-state">
            <span className="ship-empty-icon">NV</span>
            <strong>No hay naves activas</strong>
            <p>Solicita al Administrador que active una nave desde Configuración.</p>
          </div>
        ) : null}

        {!isLoading && !error && ships.length > 0 ? (
          <div className="ship-grid">
            {ships.map((ship, index) => {
              const isSelected = selectedShip?.id === ship.id
              return (
                <button type="button" className={`ship-option${isSelected ? ' is-selected' : ''}`} key={ship.id} aria-pressed={isSelected} onClick={() => handleShipSelection(ship)}>
                  <div className="ship-card-top"><span className="ship-card-index">{String(index + 1).padStart(2, '0')}</span><span className="ship-status"><span className="ship-status-dot" /> Activa</span></div>
                  <div className="ship-card-body"><span className="ship-card-label">Nave</span><strong>{ship.name}</strong></div>
                  <div className="ship-card-footer"><span>{isSelected ? 'Nave seleccionada' : 'Seleccionar nave'}</span><span className="ship-card-arrow" aria-hidden="true">→</span></div>
                </button>
              )
            })}
          </div>
        ) : null}
      </section>
    </main>
  )
}

function HomeRoute() {
  const { user } = useAuth()
  return <Navigate to={user?.role === 'Management' ? '/management' : '/ships'} replace />
}

function OperationalGate({ children }: { children: React.ReactNode }) {
  const { user } = useAuth()
  if (user?.role === 'Management') return <Navigate to="/management" replace />
  return <>{children}</>
}

function ShiftRoute() {
  const { selectedShip } = useOperation()
  return selectedShip ? <ShiftStartPage /> : <Navigate to="/ships" replace />
}

function OperationsRoute() {
  const { selectedShip, activeShift } = useOperation()
  if (!selectedShip) return <Navigate to="/ships" replace />
  if (!activeShift) return <Navigate to="/shift" replace />
  return <OperationsPage />
}

function HistoryRoute() {
  const { selectedShip, activeShift } = useOperation()
  if (!selectedShip) return <Navigate to="/ships" replace />
  if (!activeShift) return <Navigate to="/shift" replace />
  return <HistoryPage />
}

function ReportsRoute() {
  const { selectedShip, activeShift } = useOperation()
  if (!selectedShip) return <Navigate to="/ships" replace />
  if (!activeShift) return <Navigate to="/shift" replace />
  return <ReportsPage />
}

export default function App() {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) return <div className="session-loading">Cargando sesión…</div>

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <HomeRoute /> : <LoginPage />} />
      <Route path="/ships" element={<ProtectedRoute><OperationalGate><ShipsPage /></OperationalGate></ProtectedRoute>} />
      <Route path="/shift" element={<ProtectedRoute><OperationalGate><ShiftRoute /></OperationalGate></ProtectedRoute>} />
      <Route path="/admin" element={<ProtectedRoute><OperationalGate><AdminPage /></OperationalGate></ProtectedRoute>} />
      <Route path="/operations" element={<ProtectedRoute><OperationalGate><OperationsRoute /></OperationalGate></ProtectedRoute>} />
      <Route path="/history" element={<ProtectedRoute><OperationalGate><HistoryRoute /></OperationalGate></ProtectedRoute>} />
      <Route path="/reports" element={<ProtectedRoute><OperationalGate><ReportsRoute /></OperationalGate></ProtectedRoute>} />
      <Route path="/management" element={<ProtectedRoute><ManagementReportsPage /></ProtectedRoute>} />
      <Route path="/app" element={isAuthenticated ? <HomeRoute /> : <Navigate to="/login" replace />} />
      <Route path="/" element={isAuthenticated ? <HomeRoute /> : <Navigate to="/login" replace />} />
      <Route path="*" element={isAuthenticated ? <HomeRoute /> : <Navigate to="/login" replace />} />
    </Routes>
  )
}
