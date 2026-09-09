import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useOperation } from '../../context/OperationContext'
import { getOpenShift, startShift } from '../../services/shiftService'
import type { Shift, ShiftType } from '../../types/shifts'
import './ShiftStartPage.css'

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

function getErrorMessage(error: unknown): string {
  if (typeof error === 'object' && error !== null) {
    const maybeError = error as {
      response?: {
        status?: number
        data?: {
          message?: string
          title?: string
        }
      }
      message?: string
    }

    if (maybeError.response?.data?.message) {
      return maybeError.response.data.message
    }

    if (maybeError.response?.data?.title) {
      return maybeError.response.data.title
    }

    if (maybeError.response?.status === 409) {
      return 'Ya existe un turno abierto para esta nave. Revise el turno actual antes de iniciar uno nuevo.'
    }

    if (maybeError.response?.status === 400) {
      return 'La información del turno no es válida. Revise la fecha y el tipo de turno.'
    }

    if (maybeError.message) {
      return maybeError.message
    }
  }

  return 'No se pudo completar la operación del turno.'
}

export function ShiftStartPage() {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const { selectedShip, activeShift, setActiveShift, clearOperation } = useOperation()
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [openShift, setOpenShift] = useState<Shift | null>(activeShift)
  const [form, setForm] = useState({
    shiftDate: getLocalDateString(),
    shiftType: 1 as ShiftType,
  })

  useEffect(() => {
    const shipId = selectedShip?.id ?? ''

    if (!shipId) {
      navigate('/ships', { replace: true })
      return
    }

    let isMounted = true

    async function loadOpenShift() {
      setError('')
      setIsLoading(true)

      try {
        const shift = await getOpenShift(shipId)

        if (!isMounted) return

        if (shift) {
          setOpenShift(shift)
          setActiveShift(shift)
          return
        }

        setOpenShift(null)
        setForm({
          shiftDate: getLocalDateString(),
          shiftType: 1,
        })
      } catch (requestError) {
        if (!isMounted) return
        setOpenShift(null)
        setError(getErrorMessage(requestError))
      } finally {
        if (isMounted) setIsLoading(false)
      }
    }

    void loadOpenShift()

    return () => {
      isMounted = false
    }
  }, [navigate, selectedShip, setActiveShift])

  function handleChangeShip() {
    clearOperation()
    navigate('/ships', { replace: true })
  }

  function handleLogout() {
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
      const createdShift = await startShift({
        shipId: selectedShip.id,
        shiftDate: form.shiftDate,
        shiftType: form.shiftType,
      })

      setOpenShift(createdShift)
      setActiveShift(createdShift)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setIsSubmitting(false)
    }
  }

  const visibleShift = openShift ?? activeShift

  return (
    <main className="shift-page">
      <section className="shift-panel" aria-labelledby="shift-title">
        <span className="shift-brand">IQBF Control</span>
        <h1 id="shift-title">Inicio de Turno</h1>

        {isLoading ? (
          <p className="status-message">Comprobando turno abierto...</p>
        ) : null}

        {error ? (
          <p className="status-message error-message" role="alert">
            {error}
          </p>
        ) : null}

        {!isLoading && visibleShift ? (
          <>
            <p className="status-badge">Turno abierto encontrado</p>
            <div className="shift-summary" aria-live="polite">
              <div className="summary-row">
                <span className="summary-label">Nave</span>
                <strong>{visibleShift.shipName}</strong>
              </div>
              <div className="summary-row">
                <span className="summary-label">Fecha</span>
                <strong>{visibleShift.shiftDate}</strong>
              </div>
              <div className="summary-row">
                <span className="summary-label">Turno</span>
                <strong>{localshiftTypeLabels[visibleShift.shiftType]}</strong>
              </div>
              <div className="summary-row">
                <span className="summary-label">Estado</span>
                <strong>{visibleShift.status === 1 ? 'Abierto' : 'Cerrado'}</strong>
              </div>
            </div>
            <div className="shift-actions">
              <button type="button" className="shift-secondary-action" onClick={handleChangeShip}>
                Cambiar nave
              </button>
              <button type="button" className="shift-primary-action" onClick={() => navigate('/operations')}>
                Continuar operación
              </button>
            </div>
          </>
        ) : null}

        {!isLoading && !visibleShift ? (
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
                <option value="">Selecciona un turno</option>
                {Object.entries(localshiftTypeLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </label>

            <div className="shift-actions">
              <button type="button" className="shift-secondary-action" onClick={handleChangeShip}>
                Cambiar nave
              </button>
              <button type="submit" className="shift-primary-action" disabled={isSubmitting}>
                {isSubmitting ? 'Iniciando turno...' : 'Iniciar turno'}
              </button>
            </div>
          </form>
        ) : null}

        {!isLoading && !visibleShift ? (
          <div className="shift-actions inline-logout">
            <button type="button" className="shift-ghost-action" onClick={handleLogout}>
              Cerrar sesión
            </button>
          </div>
        ) : null}
      </section>
    </main>
  )
}