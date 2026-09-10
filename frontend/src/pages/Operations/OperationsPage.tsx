import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useOperation } from '../../context/OperationContext'
import { getActiveBLsByShip } from '../../services/blService'
import { getShipSummary, getShiftSummary } from '../../services/dashboardService'
import { createReception } from '../../services/receptionService'
import { createDispatch } from '../../services/dispatchService'
import { uploadDispatchPhoto, uploadReceptionPhoto } from '../../services/photoService'
import type { BL } from '../../types/bls'
import type { ShipSummary, ShiftSummary } from '../../types/dashboard'
import './OperationsPage.css'

function formatQuantity(quantity: number): string {
  return new Intl.NumberFormat('es-ES', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 3,
  }).format(quantity)
}

function getErrorMessage(error: unknown): string {
  if (typeof error === 'object' && error !== null) {
    const maybeError = error as {
      response?: {
        status?: number
        data?: {
          error?: string
          message?: string
          title?: string
        }
      }
      message?: string
    }
    const responseData = maybeError.response?.data

    if (responseData?.error) return responseData.error
    if (responseData?.message) return responseData.message
    if (responseData?.title) return responseData.title

    switch (maybeError.response?.status) {
      case 400:
        return 'La información enviada no es válida.'
      case 401:
        return 'La sesión ha expirado. Inicia sesión nuevamente.'
      case 403:
        return 'No tienes permisos para realizar esta operación.'
      case 404:
        return 'No se encontró la información solicitada.'
      case 409:
        return 'La recepción no puede registrarse porque entra en conflicto con el estado actual.'
    }

    if (maybeError.message === 'Network Error') {
      return 'No se pudo conectar con el servidor. Comprueba que la API esté disponible.'
    }

    if (maybeError.message) return maybeError.message
  }

  return 'No se pudo completar la operación.'
}

function getShiftTypeLabel(shiftType: number): string {
  return shiftType === 1 ? 'Día · 06:00–18:00' : 'Noche · 18:00–06:00'
}

interface SummaryMetricProps {
  label: string
  value: string
}

function SummaryMetric({ label, value }: SummaryMetricProps) {
  return (
    <div className="operations-metric">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

export function OperationsPage() {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const { selectedShip, activeShift, clearOperation } = useOperation()
  const [bls, setBls] = useState<BL[]>([])
  const [shipSummary, setShipSummary] = useState<ShipSummary | null>(null)
  const [summary, setSummary] = useState<ShiftSummary | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSummaryRefreshing, setIsSummaryRefreshing] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isDispatchSubmitting, setIsDispatchSubmitting] = useState(false)
  const [dispatchError, setDispatchError] = useState('')
  const [dispatchSuccessMessage, setDispatchSuccessMessage] = useState('')
  const [receptionPhotos, setReceptionPhotos] = useState<File[]>([])
  const [dispatchPhotos, setDispatchPhotos] = useState<File[]>([])
  const [error, setError] = useState('')
  const [summaryError, setSummaryError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [form, setForm] = useState({
    terminalTruck: '',
    blId: '',
    quantity: '',
    comment: '',
  })
  const [dispatchForm, setDispatchForm] = useState({
    plate: '',
    blId: '',
    quantity: '',
    comment: '',
  })

  useEffect(() => {
    if (!selectedShip) {
      navigate('/ships', { replace: true })
      return
    }

    const shiftId = activeShift?.id ?? ''
    const shipId = activeShift?.shipId ?? ''

    if (!shiftId || !shipId) {
      navigate('/shift', { replace: true })
      return
    }

    let isMounted = true

    async function loadOperationData() {
      setIsLoading(true)
      setError('')
      setSummaryError('')

      try {
        const [activeBLsResult, shipSummaryResult, shiftSummaryResult] = await Promise.allSettled([
          getActiveBLsByShip(shipId),
          getShipSummary(shipId),
          getShiftSummary(shiftId),
        ])

        if (!isMounted) return

        if (activeBLsResult.status === 'fulfilled') {
          setBls(activeBLsResult.value)
          setForm((current) => ({
            ...current,
            blId: activeBLsResult.value.some((bl) => bl.id === current.blId)
              ? current.blId
              : activeBLsResult.value[0]?.id ?? '',
          }))
          setDispatchForm((current) => ({
            ...current,
            blId: activeBLsResult.value.some((bl) => bl.id === current.blId)
              ? current.blId
              : activeBLsResult.value[0]?.id ?? '',
          }))
        } else {
          setError(getErrorMessage(activeBLsResult.reason))
        }

        const summaryErrors: string[] = []
        if (shipSummaryResult.status === 'fulfilled') {
          setShipSummary(shipSummaryResult.value)
        } else {
          summaryErrors.push(`Acumulado de nave: ${getErrorMessage(shipSummaryResult.reason)}`)
        }

        if (shiftSummaryResult.status === 'fulfilled') {
          setSummary(shiftSummaryResult.value)
        } else {
          summaryErrors.push(`Turno actual: ${getErrorMessage(shiftSummaryResult.reason)}`)
        }

        setSummaryError(summaryErrors.join(' '))
      } finally {
        if (isMounted) setIsLoading(false)
      }
    }

    void loadOperationData()

    return () => {
      isMounted = false
    }
  }, [activeShift, navigate, selectedShip])

  async function refreshSummaries() {
    if (!activeShift) return

    setIsSummaryRefreshing(true)
    setSummaryError('')

    const [shipSummaryResult, shiftSummaryResult] = await Promise.allSettled([
      getShipSummary(activeShift.shipId),
      getShiftSummary(activeShift.id),
    ])

    if (shipSummaryResult.status === 'fulfilled') setShipSummary(shipSummaryResult.value)
    if (shiftSummaryResult.status === 'fulfilled') setSummary(shiftSummaryResult.value)

    const summaryErrors: string[] = []
    if (shipSummaryResult.status === 'rejected') {
      summaryErrors.push(`Acumulado de nave: ${getErrorMessage(shipSummaryResult.reason)}`)
    }
    if (shiftSummaryResult.status === 'rejected') {
      summaryErrors.push(`Turno actual: ${getErrorMessage(shiftSummaryResult.reason)}`)
    }
    setSummaryError(summaryErrors.join(' '))
    setIsSummaryRefreshing(false)
  }

  function handleLogout() {
    clearOperation()
    logout()
  }

  function validatePhotos(files: File[]): string {
    if (files.length > 3) return 'Puedes adjuntar como máximo 3 fotografías.'
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp']
    const invalid = files.find((file) => !allowedTypes.includes(file.type) || file.size > 10 * 1024 * 1024)
    if (invalid) return 'Las fotografías deben ser JPG, PNG o WEBP y no superar 10 MB cada una.'
    return ''
  }

  async function handleReceptionSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!activeShift || !form.blId) return

    const quantity = Number(form.quantity)
    if (!Number.isFinite(quantity) || quantity <= 0) {
      setError('La cantidad debe ser un número mayor que cero.')
      return
    }

    const photoError = validatePhotos(receptionPhotos)
    if (photoError) {
      setError(photoError)
      return
    }

    setIsSubmitting(true)
    setError('')
    setSuccessMessage('')

    try {
      const reception = await createReception({
        shiftId: activeShift.id,
        terminalTruck: form.terminalTruck,
        comment: form.comment || undefined,
        items: [{
          blId: form.blId,
          quantity,
        }],
      })

      let uploadedPhotos = 0
      let photoUploadError = ''

      for (const photo of receptionPhotos) {
        try {
          await uploadReceptionPhoto(reception.id, photo)
          uploadedPhotos += 1
        } catch (requestError) {
          photoUploadError = getErrorMessage(requestError)
          break
        }
      }

      setForm((current) => ({
        ...current,
        terminalTruck: '',
        quantity: '',
        comment: '',
      }))
      setReceptionPhotos([])
      await refreshSummaries()

      if (photoUploadError) {
        setSuccessMessage(`Recepción registrada correctamente. Transacción #${reception.transactionNumber}. No vuelva a registrar la recepción.`)
        setError(`La recepción quedó guardada, pero falló la evidencia fotográfica después de cargar ${uploadedPhotos} de ${receptionPhotos.length} foto(s): ${photoUploadError}`)
      } else {
        setSuccessMessage(`Recepción registrada correctamente. Transacción #${reception.transactionNumber}.${uploadedPhotos ? ` ${uploadedPhotos} foto(s) cargada(s).` : ''}`)
      }
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleDispatchSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!activeShift || !dispatchForm.blId) return

    const quantity = Number(dispatchForm.quantity)
    if (!Number.isFinite(quantity) || quantity <= 0) {
      setDispatchError('La cantidad debe ser un número mayor que cero.')
      return
    }

    const photoError = validatePhotos(dispatchPhotos)
    if (photoError) {
      setDispatchError(photoError)
      return
    }

    setIsDispatchSubmitting(true)
    setDispatchError('')
    setDispatchSuccessMessage('')

    try {
      const dispatch = await createDispatch({
        shiftId: activeShift.id,
        plate: dispatchForm.plate,
        comment: dispatchForm.comment || undefined,
        items: [{
          blId: dispatchForm.blId,
          quantity,
        }],
      })

      let uploadedPhotos = 0
      let photoUploadError = ''

      for (const photo of dispatchPhotos) {
        try {
          await uploadDispatchPhoto(dispatch.id, photo)
          uploadedPhotos += 1
        } catch (requestError) {
          photoUploadError = getErrorMessage(requestError)
          break
        }
      }

      setDispatchForm((current) => ({
        ...current,
        plate: '',
        quantity: '',
        comment: '',
      }))
      setDispatchPhotos([])
      await refreshSummaries()

      if (photoUploadError) {
        setDispatchSuccessMessage(`Despacho registrado correctamente. Transacción #${dispatch.transactionNumber}. No vuelva a registrar el despacho.`)
        setDispatchError(`El despacho quedó guardado, pero falló la evidencia fotográfica después de cargar ${uploadedPhotos} de ${dispatchPhotos.length} foto(s): ${photoUploadError}`)
      } else {
        setDispatchSuccessMessage(`Despacho registrado correctamente. Transacción #${dispatch.transactionNumber}.${uploadedPhotos ? ` ${uploadedPhotos} foto(s) cargada(s).` : ''}`)
      }
    } catch (requestError) {
      setDispatchError(getErrorMessage(requestError))
    } finally {
      setIsDispatchSubmitting(false)
    }
  }

  return (
    <main className="operations-shell">
      <header className="operations-header">
        <div>
          <span className="eyebrow">IQBF Control</span>
          <h1>Operación en curso</h1>
          <p className="operations-context">
            {selectedShip?.name} · {activeShift ? getShiftTypeLabel(activeShift.shiftType) : 'Sin turno'}
          </p>
        </div>
        <button type="button" className="secondary-action" onClick={handleLogout}>
          Cerrar sesión
        </button>
      </header>

      <div className="operations-grid">
        <section className="operations-column" aria-labelledby="reception-title">
          <div className="operations-column-heading">
            <span className="eyebrow">Bloque 01</span>
            <h2 id="reception-title">Recepción</h2>
          </div>

          {isLoading ? <p className="operations-message">Cargando BL y resumen...</p> : null}
          {error ? <p className="operations-message operations-error" role="alert">{error}</p> : null}
          {successMessage ? <p className="operations-message operations-success" role="status">{successMessage}</p> : null}

          {!isLoading ? (
            <form className="reception-form" onSubmit={handleReceptionSubmit}>
              <label className="operations-field">
                <span>Terminal Truck</span>
                <input
                  type="text"
                  value={form.terminalTruck}
                  onChange={(event) => setForm((current) => ({ ...current, terminalTruck: event.target.value }))}
                  inputMode="numeric"
                  maxLength={30}
                  required
                />
              </label>

              <label className="operations-field">
                <span>BL</span>
                <select
                  value={form.blId}
                  onChange={(event) => setForm((current) => ({ ...current, blId: event.target.value }))}
                  required
                  disabled={bls.length === 0}
                >
                  <option value="">Selecciona un BL</option>
                  {bls.map((bl) => (
                    <option key={bl.id} value={bl.id}>
                      {bl.code} · {bl.productName} · declarado {formatQuantity(bl.totalQuantity)}
                    </option>
                  ))}
                </select>
              </label>

              <label className="operations-field">
                <span>Cantidad</span>
                <input
                  type="number"
                  value={form.quantity}
                  onChange={(event) => setForm((current) => ({ ...current, quantity: event.target.value }))}
                  min="0.001"
                  step="0.001"
                  required
                />
              </label>

              <label className="operations-field">
                <span>Comentario <small>(opcional)</small></span>
                <textarea
                  value={form.comment}
                  onChange={(event) => setForm((current) => ({ ...current, comment: event.target.value }))}
                  maxLength={100}
                  rows={3}
                />
              </label>

              <label className="operations-field">
                <span>Evidencia fotográfica <small>(opcional · máximo 3)</small></span>
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  multiple
                  onChange={(event) => setReceptionPhotos(Array.from(event.target.files ?? []).slice(0, 3))}
                />
                {receptionPhotos.length ? <small>{receptionPhotos.length} foto(s) seleccionada(s)</small> : null}
              </label>

              <button type="submit" className="operations-primary-action" disabled={isSubmitting || bls.length === 0}>
                {isSubmitting ? 'Guardando recepción...' : 'Registrar recepción'}
              </button>
              {bls.length === 0 ? <p className="operations-hint">No hay BL activos para esta nave.</p> : null}
            </form>
          ) : null}
        </section>

        <section className="operations-column dispatch-column" aria-labelledby="dispatch-title">
          <div className="operations-column-heading">
            <span className="eyebrow">Bloque 02</span>
            <h2 id="dispatch-title">Despacho</h2>
          </div>
          {dispatchError ? <p className="operations-message operations-error" role="alert">{dispatchError}</p> : null}
          {dispatchSuccessMessage ? <p className="operations-message operations-success" role="status">{dispatchSuccessMessage}</p> : null}

          <form className="reception-form" onSubmit={handleDispatchSubmit}>
            <label className="operations-field">
              <span>Placa</span>
              <input
                type="text"
                value={dispatchForm.plate}
                onChange={(event) => setDispatchForm((current) => ({ ...current, plate: event.target.value }))}
                maxLength={20}
                required
              />
            </label>

            <label className="operations-field">
              <span>BL</span>
              <select
                value={dispatchForm.blId}
                onChange={(event) => setDispatchForm((current) => ({ ...current, blId: event.target.value }))}
                required
                disabled={bls.length === 0}
              >
                <option value="">Selecciona un BL</option>
                {bls.map((bl) => (
                  <option key={bl.id} value={bl.id}>
                    {bl.code} · {bl.productName} · declarado {formatQuantity(bl.totalQuantity)}
                  </option>
                ))}
              </select>
            </label>

            <label className="operations-field">
              <span>Cantidad</span>
              <input
                type="number"
                value={dispatchForm.quantity}
                onChange={(event) => setDispatchForm((current) => ({ ...current, quantity: event.target.value }))}
                min="0.001"
                step="0.001"
                required
              />
            </label>

            <label className="operations-field">
              <span>Comentario <small>(opcional)</small></span>
              <textarea
                value={dispatchForm.comment}
                onChange={(event) => setDispatchForm((current) => ({ ...current, comment: event.target.value }))}
                maxLength={100}
                rows={3}
              />
            </label>

            <label className="operations-field">
              <span>Evidencia fotográfica <small>(opcional · máximo 3)</small></span>
              <input
                type="file"
                accept="image/jpeg,image/png,image/webp"
                multiple
                onChange={(event) => setDispatchPhotos(Array.from(event.target.files ?? []).slice(0, 3))}
              />
              {dispatchPhotos.length ? <small>{dispatchPhotos.length} foto(s) seleccionada(s)</small> : null}
            </label>

            <button type="submit" className="operations-primary-action" disabled={isDispatchSubmitting || bls.length === 0}>
              {isDispatchSubmitting ? 'Guardando despacho...' : 'Registrar despacho'}
            </button>
            {bls.length === 0 ? <p className="operations-hint">No hay BL activos para esta nave.</p> : null}
          </form>
        </section>

        <section className="operations-column summary-column" aria-labelledby="summary-title">
          <div className="operations-column-heading">
            <span className="eyebrow">En vivo</span>
            <h2 id="summary-title">Resumen</h2>
          </div>

          {summaryError ? <p className="operations-message operations-error" role="alert">{summaryError}</p> : null}
          {isSummaryRefreshing ? <p className="operations-message">Actualizando resúmenes...</p> : null}

          {shipSummary ? (
            <>
              <section className="summary-section summary-primary" aria-labelledby="ship-summary-title">
                <div className="summary-section-heading">
                  <span className="eyebrow">Acumulado operativo</span>
                  <h3 id="ship-summary-title">Acumulado de nave</h3>
                </div>
                <div className="shift-details">
                  <div><span>Nave</span><strong>{shipSummary.shipName}</strong></div>
                </div>
                <div className="operations-metrics">
                  <SummaryMetric label="Total declarado" value={formatQuantity(shipSummary.totalQuantity)} />
                  <SummaryMetric label="Total recibido" value={formatQuantity(shipSummary.receivedQuantity)} />
                  <SummaryMetric label="Total despachado" value={formatQuantity(shipSummary.dispatchedQuantity)} />
                  <SummaryMetric label="Disponible" value={formatQuantity(shipSummary.availableQuantity)} />
                  <SummaryMetric label="Pendiente de recepción" value={formatQuantity(shipSummary.pendingReception)} />
                </div>

                <div className="bl-summary-list">
                  <h3>Detalle acumulado por BL</h3>
                  {shipSummary.bLs.length === 0 ? (
                    <p className="operations-hint">No hay BL registrados para esta nave.</p>
                  ) : shipSummary.bLs.map((balance) => {
                  return (
                    <article className="bl-summary-card" key={balance.id}>
                      <div className="bl-summary-heading">
                        <strong>{balance.code}</strong>
                        <span>{balance.productName}</span>
                      </div>
                      <div className="bl-summary-values">
                        <span>Declarado <strong>{formatQuantity(balance.totalQuantity)}</strong></span>
                        <span>Recibido <strong>{formatQuantity(balance.receivedQuantity)}</strong></span>
                        <span>Despachado <strong>{formatQuantity(balance.dispatchedQuantity)}</strong></span>
                        <span>Disponible <strong>{formatQuantity(balance.availableQuantity)}</strong></span>
                        <span>Pendiente de recepción <strong>{formatQuantity(balance.pendingReception)}</strong></span>
                      </div>
                    </article>
                  )
                })}
                </div>
              </section>

              {summary ? (
                <section className="summary-section" aria-labelledby="shift-summary-title">
                  <div className="summary-section-heading">
                    <span className="eyebrow">Movimiento del período</span>
                    <h3 id="shift-summary-title">Turno actual</h3>
                  </div>
                  <div className="shift-details">
                    <div><span>Fecha</span><strong>{summary.shiftDate}</strong></div>
                    <div><span>Horario</span><strong>{getShiftTypeLabel(summary.shiftType)}</strong></div>
                  </div>
                  <div className="operations-metrics">
                    <SummaryMetric label="Recibido en el turno" value={formatQuantity(summary.receivedQuantity)} />
                    <SummaryMetric label="Despachado en el turno" value={formatQuantity(summary.dispatchedQuantity)} />
                  </div>
                </section>
              ) : null}
            </>
          ) : !isLoading && !summaryError ? (
            <p className="operations-message">No se pudo cargar el resumen de la nave.</p>
          ) : null}
        </section>
      </div>
    </main>
  )
}
