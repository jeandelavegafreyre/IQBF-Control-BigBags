import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useOperation } from '../../context/OperationContext'
import { startShift } from '../../services/shiftService'
import type { ShiftType } from '../../types/shifts'
import './ShiftStartPage.css'

const SHIFT_DATE_STORAGE = 'operationalShiftDate'
const SHIFT_TYPE_STORAGE = 'operationalShiftType'

const localshiftTypeLabels: Record<ShiftType, string> = {
  1: 'Día — 06:00–18:00',
  2: 'Noche — 18:00–06:00',
}

function getLocalDateString(date = new Date()): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function getStoredShiftType(): ShiftType {
  const stored = sessionStorage.getItem(SHIFT_TYPE_STORAGE)
  return stored === '2' ? 2 : 1
}

function getErrorMessage(error: unknown): string {
  if (typeof error === 'object' && error !== null) {
    const maybeError = error as {
      response?: {
        status?: number
        data?: { error?: string; message?: string; title?: string }
      }
      message?: string
    }
    const responseData = maybeError.response?.data
    if (responseData?.error) return responseData.error
    if (responseData?.message) return responseData.message
    if (responseData?.title) return responseData.title
    if (maybeError.response?.status === 400) return 'La información del turno no es válida. Revise la fecha y el tipo de turno.'
    if (maybeError.response?.status === 403) return 'No tienes permisos para iniciar o consultar turnos.'
    if (maybeError.response?.status === 401) return 'La sesión ha expirado. Inicia sesión nuevamente.'
    if (maybeError.response?.status === 500) return 'El servidor no pudo preparar el turno operativo.'
    if (maybeError.message) return maybeError.message
  }
  return 'No se pudo preparar el turno operativo.'
}

export function ShiftStartPage() {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const { selectedShip, setActiveShift, clearOperation } = useOperation()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [form, setForm] = useState({
    shiftDate: sessionStorage.getItem(SHIFT_DATE_STORAGE) || getLocalDateString(),
    shiftType: getStoredShiftType(),
  })

  function handleChangeShip() {
    clearOperation()
    navigate('/ships', { replace: true })
  }

  function handleLogout() {
    sessionStorage.removeItem(SHIFT_DATE_STORAGE)
    sessionStorage.removeItem(SHIFT_TYPE_STORAGE)
    clearOperation()
    logout()
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedShip) {
      navigate('/ships', { replace: true })
      return
    }

    setIsSubmitting(true)
    setError('')

    try {
      const shift = await startShift({
        shipId: selectedShip.id,
        shiftDate: form.shiftDate,
        shiftType: form.shiftType,
      })

      sessionStorage.setItem(SHIFT_DATE_STORAGE, form.shiftDate)
      sessionStorage.setItem(SHIFT_TYPE_STORAGE, String(form.shiftType))
      setActiveShift(shift)
      navigate('/operations')
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="shift-page">
      <section className="shift-panel" aria-labelledby="shift-title">
        <span className="shift-brand">IQBF Control</span>
        <h1 id="shift-title">Selecciona el turno</h1>
        <p className="status-message">Elige el turno operativo para {selectedShip?.name}. Al cambiar de nave, esta selección se conservará durante tu sesión.</p>

        {error ? <p className="status-message error-message" role="alert">{error}</p> : null}

        <form className="shift-form" onSubmit={handleSubmit}>
          <label className="field-group">
            <span>Nave</span>
            <input type="text" value={selectedShip?.name ?? ''} readOnly />
          </label>

          <label className="field-group">
            <span>Fecha</span>
            <input
              type="date"
              value={form.shiftDate}
              onChange={(event) => setForm((current) => ({ ...current, shiftDate: event.target.value }))}
              required
            />
          </label>

          <label className="field-group">
            <span>Tipo de turno</span>
            <select
              value={form.shiftType}
              onChange={(event) => setForm((current) => ({ ...current, shiftType: Number(event.target.value) as ShiftType }))}
              required
            >
              {Object.entries(localshiftTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </label>

          <div className="shift-actions">
            <button type="button" className="shift-secondary-action" onClick={handleChangeShip}>Cambiar nave</button>
            <button type="submit" className="shift-primary-action" disabled={isSubmitting}>
              {isSubmitting ? 'Preparando operación...' : 'Ingresar a operación'}
            </button>
          </div>
        </form>

        <div className="shift-actions inline-logout">
          <button type="button" className="shift-ghost-action" onClick={handleLogout}>Cerrar sesión</button>
        </div>
      </section>
    </main>
  )
}
