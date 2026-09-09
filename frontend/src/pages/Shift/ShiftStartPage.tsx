import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useOperation } from '../../context/OperationContext'
import './ShiftStartPage.css'

export function ShiftStartPage() {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const { selectedShip, clearOperation } = useOperation()

  function handleChangeShip() {
    clearOperation()
    navigate('/ships', { replace: true })
  }

  function handleLogout() {
    clearOperation()
    logout()
  }

  return (
    <main className="shift-page">
      <section className="shift-panel" aria-labelledby="shift-title">
        <span className="shift-brand">IQBF Control</span>
        <h1 id="shift-title">Inicio de Turno</h1>
        <p className="shift-ship">Nave seleccionada: {selectedShip?.name}</p>
        <p className="shift-pending">Configuración del turno pendiente</p>
        <div className="shift-actions">
          <button type="button" className="shift-secondary-action" onClick={handleChangeShip}>
            Cambiar nave
          </button>
          <button type="button" className="shift-primary-action" onClick={handleLogout}>
            Cerrar sesión
          </button>
        </div>
      </section>
    </main>
  )
}