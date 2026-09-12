import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useOperation } from '../../context/OperationContext'
import { getActiveBLsByShip } from '../../services/blService'
import { getShipSummary, getShiftSummary } from '../../services/dashboardService'
import { createReception } from '../../services/receptionService'
import { createDispatch } from '../../services/dispatchService'
import { uploadDispatchPhoto, uploadReceptionPhoto } from '../../services/photoService'
import { createOperationsConnection } from '../../services/operationsHubService'
import type { BL } from '../../types/bls'
import type { ShipSummary, ShiftSummary } from '../../types/dashboard'
import { SummaryDashboard } from './SummaryDashboard'
import './OperationsPage.css'

type ReceptionLine = { blId: string; quantity: string }

function formatQuantity(quantity: number): string {
  return new Intl.NumberFormat('es-ES', { minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(quantity)
}

function getErrorMessage(error: unknown): string {
  if (typeof error === 'object' && error !== null) {
    const maybeError = error as { response?: { status?: number; data?: { error?: string; message?: string; title?: string } }; message?: string }
    const responseData = maybeError.response?.data
    if (responseData?.error) return responseData.error
    if (responseData?.message) return responseData.message
    if (responseData?.title) return responseData.title
    switch (maybeError.response?.status) {
      case 400: return 'La información enviada no es válida.'
      case 401: return 'La sesión ha expirado. Inicia sesión nuevamente.'
      case 403: return 'No tienes permisos para realizar esta operación.'
      case 404: return 'No se encontró la información solicitada.'
      case 409: return 'La operación entra en conflicto con el estado actual.'
    }
    if (maybeError.message === 'Network Error') return 'No se pudo conectar con el servidor. Comprueba que la API esté disponible.'
    if (maybeError.message) return maybeError.message
  }
  return 'No se pudo completar la operación.'
}

function getShiftTypeLabel(shiftType: number): string { return shiftType === 1 ? 'Día · 06:00–18:00' : 'Noche · 18:00–06:00' }

export function OperationsPage() {
  const navigate = useNavigate()
  const { user, logout, token } = useAuth()
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
  const [form, setForm] = useState({ terminalTruck: '', comment: '' })
  const [receptionLines, setReceptionLines] = useState<ReceptionLine[]>([{ blId: '', quantity: '' }])
  const [dispatchForm, setDispatchForm] = useState({ plate: '', blId: '', quantity: '', comment: '' })

  const receptionTotal = receptionLines.reduce((total, line) => total + (Number(line.quantity) || 0), 0)
  const canChangeOperation = user?.role === 'Administrator' || user?.role === 'Yard'

  useEffect(() => {
    if (!selectedShip) { navigate('/ships', { replace: true }); return }
    const shiftId = activeShift?.id ?? ''; const shipId = activeShift?.shipId ?? ''
    if (!shiftId || !shipId) { navigate('/shift', { replace: true }); return }
    let isMounted = true
    async function loadOperationData() {
      setIsLoading(true); setError(''); setSummaryError('')
      try {
        const [activeBLsResult, shipSummaryResult, shiftSummaryResult] = await Promise.allSettled([getActiveBLsByShip(shipId), getShipSummary(shipId), getShiftSummary(shiftId)])
        if (!isMounted) return
        if (activeBLsResult.status === 'fulfilled') {
          setBls(activeBLsResult.value)
          const firstBlId = activeBLsResult.value[0]?.id ?? ''
          setReceptionLines((current) => current.map((line, index) => ({ ...line, blId: activeBLsResult.value.some((bl) => bl.id === line.blId) ? line.blId : index === 0 ? firstBlId : '' })))
          setDispatchForm((current) => ({ ...current, blId: activeBLsResult.value.some((bl) => bl.id === current.blId) ? current.blId : firstBlId }))
        } else setError(getErrorMessage(activeBLsResult.reason))
        const summaryErrors: string[] = []
        if (shipSummaryResult.status === 'fulfilled') setShipSummary(shipSummaryResult.value); else summaryErrors.push(`Acumulado de nave: ${getErrorMessage(shipSummaryResult.reason)}`)
        if (shiftSummaryResult.status === 'fulfilled') setSummary(shiftSummaryResult.value); else summaryErrors.push(`Turno actual: ${getErrorMessage(shiftSummaryResult.reason)}`)
        setSummaryError(summaryErrors.join(' '))
      } finally { if (isMounted) setIsLoading(false) }
    }
    void loadOperationData(); return () => { isMounted = false }
  }, [activeShift, navigate, selectedShip])

  useEffect(() => {
    if (!token || !activeShift) return
    const connection = createOperationsConnection(token); let disposed = false
    const refreshFromHub = () => { if (!disposed) void refreshSummaries() }
    connection.on('ReceptionCreated', refreshFromHub); connection.on('DispatchCreated', refreshFromHub); connection.start().catch(() => {})
    return () => { disposed = true; connection.off('ReceptionCreated', refreshFromHub); connection.off('DispatchCreated', refreshFromHub); void connection.stop() }
  }, [activeShift?.id, token])

  async function refreshSummaries() {
    if (!activeShift) return
    setIsSummaryRefreshing(true); setSummaryError('')
    const [shipSummaryResult, shiftSummaryResult] = await Promise.allSettled([getShipSummary(activeShift.shipId), getShiftSummary(activeShift.id)])
    if (shipSummaryResult.status === 'fulfilled') setShipSummary(shipSummaryResult.value)
    if (shiftSummaryResult.status === 'fulfilled') setSummary(shiftSummaryResult.value)
    const summaryErrors: string[] = []
    if (shipSummaryResult.status === 'rejected') summaryErrors.push(`Acumulado de nave: ${getErrorMessage(shipSummaryResult.reason)}`)
    if (shiftSummaryResult.status === 'rejected') summaryErrors.push(`Turno actual: ${getErrorMessage(shiftSummaryResult.reason)}`)
    setSummaryError(summaryErrors.join(' ')); setIsSummaryRefreshing(false)
  }

  function handleChangeOperation() {
    if (!canChangeOperation || isSubmitting || isDispatchSubmitting) return
    clearOperation()
    navigate('/ships')
  }

  function handleLogout() { clearOperation(); logout() }
  function updateReceptionLine(index: number, field: keyof ReceptionLine, value: string) {
    setReceptionLines((current) => current.map((line, lineIndex) => lineIndex === index ? { ...line, [field]: value } : line))
  }
  function addReceptionLine() {
    const usedIds = new Set(receptionLines.map((line) => line.blId).filter(Boolean))
    const nextBl = bls.find((bl) => !usedIds.has(bl.id))
    setReceptionLines((current) => [...current, { blId: nextBl?.id ?? '', quantity: '' }])
  }
  function removeReceptionLine(index: number) {
    setReceptionLines((current) => current.length === 1 ? current : current.filter((_, lineIndex) => lineIndex !== index))
  }
  function validatePhotos(files: File[]): string {
    if (files.length > 3) return 'Puedes adjuntar como máximo 3 fotografías.'
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp']; const invalid = files.find((file) => !allowedTypes.includes(file.type) || file.size > 10 * 1024 * 1024)
    return invalid ? 'Las fotografías deben ser JPG, PNG o WEBP y no superar 10 MB cada una.' : ''
  }

  async function handleReceptionSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (!activeShift) return
    if (!/^\d+$/.test(form.terminalTruck.trim())) { setError('Terminal Truck debe contener únicamente números.'); return }
    if (receptionLines.some((line) => !line.blId)) { setError('Selecciona un BL en todas las líneas de recepción.'); return }
    const blIds = receptionLines.map((line) => line.blId)
    if (new Set(blIds).size !== blIds.length) { setError('No se puede repetir el mismo BL dentro de una recepción.'); return }
    const items = receptionLines.map((line) => ({ blId: line.blId, quantity: Number(line.quantity) }))
    if (items.some((item) => !Number.isInteger(item.quantity) || item.quantity <= 0)) { setError('Todas las cantidades deben ser números enteros de Big Bags mayores que cero.'); return }
    const photoError = validatePhotos(receptionPhotos); if (photoError) { setError(photoError); return }
    setIsSubmitting(true); setError(''); setSuccessMessage('')
    try {
      const reception = await createReception({ shiftId: activeShift.id, terminalTruck: form.terminalTruck.trim(), comment: form.comment || undefined, items })
      let uploadedPhotos = 0; let photoUploadError = ''
      for (const photo of receptionPhotos) { try { await uploadReceptionPhoto(reception.id, photo); uploadedPhotos += 1 } catch (requestError) { photoUploadError = getErrorMessage(requestError); break } }
      setForm({ terminalTruck: '', comment: '' }); setReceptionLines([{ blId: bls[0]?.id ?? '', quantity: '' }]); setReceptionPhotos([]); await refreshSummaries()
      if (photoUploadError) { setSuccessMessage(`Recepción registrada correctamente. Transacción #${reception.transactionNumber}. No vuelva a registrar la recepción.`); setError(`La recepción quedó guardada, pero falló la evidencia fotográfica después de cargar ${uploadedPhotos} de ${receptionPhotos.length} foto(s): ${photoUploadError}`) }
      else setSuccessMessage(`Recepción registrada correctamente. Transacción #${reception.transactionNumber}.${uploadedPhotos ? ` ${uploadedPhotos} foto(s) cargada(s).` : ''}`)
    } catch (requestError) { setError(getErrorMessage(requestError)) } finally { setIsSubmitting(false) }
  }

  async function handleDispatchSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (!activeShift || !dispatchForm.blId) return
    const quantity = Number(dispatchForm.quantity)
    if (!Number.isInteger(quantity) || quantity <= 0) { setDispatchError('La cantidad debe ser un número entero de Big Bags mayor que cero.'); return }
    const photoError = validatePhotos(dispatchPhotos); if (photoError) { setDispatchError(photoError); return }
    setIsDispatchSubmitting(true); setDispatchError(''); setDispatchSuccessMessage('')
    try {
      const dispatch = await createDispatch({ shiftId: activeShift.id, plate: dispatchForm.plate, comment: dispatchForm.comment || undefined, items: [{ blId: dispatchForm.blId, quantity }] })
      let uploadedPhotos = 0; let photoUploadError = ''
      for (const photo of dispatchPhotos) { try { await uploadDispatchPhoto(dispatch.id, photo); uploadedPhotos += 1 } catch (requestError) { photoUploadError = getErrorMessage(requestError); break } }
      setDispatchForm((current) => ({ ...current, plate: '', quantity: '', comment: '' })); setDispatchPhotos([]); await refreshSummaries()
      if (photoUploadError) { setDispatchSuccessMessage(`Despacho registrado correctamente. Transacción #${dispatch.transactionNumber}. No vuelva a registrar el despacho.`); setDispatchError(`El despacho quedó guardado, pero falló la evidencia fotográfica después de cargar ${uploadedPhotos} de ${dispatchPhotos.length} foto(s): ${photoUploadError}`) }
      else setDispatchSuccessMessage(`Despacho registrado correctamente. Transacción #${dispatch.transactionNumber}.${uploadedPhotos ? ` ${uploadedPhotos} foto(s) cargada(s).` : ''}`)
    } catch (requestError) { setDispatchError(getErrorMessage(requestError)) } finally { setIsDispatchSubmitting(false) }
  }

  return (
    <main className="operations-shell">
      <header className="operations-header">
        <div>
          <span className="eyebrow">IQBF Control</span>
          <h1>Operación en curso</h1>
          <p className="operations-context">{selectedShip?.name} · {activeShift ? getShiftTypeLabel(activeShift.shiftType) : 'Sin turno'}</p>
        </div>
        <div className="operations-header-actions">
          {canChangeOperation ? <button type="button" className="secondary-action" onClick={handleChangeOperation} disabled={isSubmitting || isDispatchSubmitting}>Cambiar nave / turno</button> : null}
          <button type="button" className="secondary-action" onClick={() => navigate('/history')} disabled={isSubmitting || isDispatchSubmitting}>Historial operativo</button>
          <button type="button" className="secondary-action" onClick={handleLogout}>Cerrar sesión</button>
        </div>
      </header>
      <div className="operations-grid">
        <section className="operations-column" aria-labelledby="reception-title"><div className="operations-column-heading"><span className="eyebrow">Bloque 01</span><h2 id="reception-title">Recepción</h2></div>
          {isLoading ? <p className="operations-message">Cargando BL y resumen...</p> : null}{error ? <p className="operations-message operations-error" role="alert">{error}</p> : null}{successMessage ? <p className="operations-message operations-success" role="status">{successMessage}</p> : null}
          {!isLoading ? <form className="reception-form" onSubmit={handleReceptionSubmit}>
            <label className="operations-field"><span>Terminal Truck</span><input type="text" value={form.terminalTruck} onChange={(event) => setForm((current) => ({ ...current, terminalTruck: event.target.value.replace(/\D/g, '') }))} inputMode="numeric" maxLength={30} required /></label>
            <div className="reception-items"><div className="reception-items-heading"><div><strong>BL recibidos</strong><small> Agrega una línea por cada BL transportado en el mismo Terminal Truck.</small></div><button type="button" className="reception-add-line" onClick={addReceptionLine} disabled={receptionLines.length >= bls.length || bls.length === 0}>+ Agregar BL</button></div>
              {receptionLines.map((line, index) => <div className="reception-item-row" key={index}><label className="operations-field"><span>BL {index + 1}</span><select value={line.blId} onChange={(event) => updateReceptionLine(index, 'blId', event.target.value)} required disabled={bls.length === 0}><option value="">Selecciona un BL</option>{bls.map((bl) => <option key={bl.id} value={bl.id} disabled={receptionLines.some((other, otherIndex) => otherIndex !== index && other.blId === bl.id)}>{bl.code} · {bl.productName} · declarado {formatQuantity(bl.totalQuantity)}</option>)}</select></label><label className="operations-field reception-quantity"><span>Cantidad</span><input type="number" value={line.quantity} onChange={(event) => updateReceptionLine(index, 'quantity', event.target.value)} min="1" step="1" inputMode="numeric" required /></label><button type="button" className="reception-remove-line" onClick={() => removeReceptionLine(index)} disabled={receptionLines.length === 1} aria-label={`Quitar BL ${index + 1}`}>Quitar</button></div>)}
              <div className="reception-total"><span>Total del Terminal Truck</span><strong>{formatQuantity(receptionTotal)} Big Bags</strong></div>
            </div>
            <label className="operations-field"><span>Comentario <small>(opcional)</small></span><textarea value={form.comment} onChange={(event) => setForm((current) => ({ ...current, comment: event.target.value }))} maxLength={100} rows={3} /></label><label className="operations-field"><span>Evidencia fotográfica <small>(opcional · máximo 3)</small></span><input type="file" accept="image/jpeg,image/png,image/webp" multiple onChange={(event) => setReceptionPhotos(Array.from(event.target.files ?? []).slice(0, 3))} />{receptionPhotos.length ? <small>{receptionPhotos.length} foto(s) seleccionada(s)</small> : null}</label><button type="submit" className="operations-primary-action" disabled={isSubmitting || bls.length === 0}>{isSubmitting ? 'Guardando recepción...' : `Registrar recepción · ${formatQuantity(receptionTotal)} Big Bags`}</button>{bls.length === 0 ? <p className="operations-hint">No hay BL activos para esta nave.</p> : null}</form> : null}
        </section>
        <section className="operations-column dispatch-column" aria-labelledby="dispatch-title"><div className="operations-column-heading"><span className="eyebrow">Bloque 02</span><h2 id="dispatch-title">Despacho</h2></div>{dispatchError ? <p className="operations-message operations-error" role="alert">{dispatchError}</p> : null}{dispatchSuccessMessage ? <p className="operations-message operations-success" role="status">{dispatchSuccessMessage}</p> : null}<form className="reception-form" onSubmit={handleDispatchSubmit}><label className="operations-field"><span>Placa</span><input type="text" value={dispatchForm.plate} onChange={(event) => setDispatchForm((current) => ({ ...current, plate: event.target.value }))} maxLength={20} required /></label><label className="operations-field"><span>BL</span><select value={dispatchForm.blId} onChange={(event) => setDispatchForm((current) => ({ ...current, blId: event.target.value }))} required disabled={bls.length === 0}><option value="">Selecciona un BL</option>{bls.map((bl) => <option key={bl.id} value={bl.id}>{bl.code} · {bl.productName} · declarado {formatQuantity(bl.totalQuantity)}</option>)}</select></label><label className="operations-field"><span>Cantidad (Big Bags)</span><input type="number" value={dispatchForm.quantity} onChange={(event) => setDispatchForm((current) => ({ ...current, quantity: event.target.value }))} min="1" step="1" inputMode="numeric" required /></label><label className="operations-field"><span>Comentario <small>(opcional)</small></span><textarea value={dispatchForm.comment} onChange={(event) => setDispatchForm((current) => ({ ...current, comment: event.target.value }))} maxLength={100} rows={3} /></label><label className="operations-field"><span>Evidencia fotográfica <small>(opcional · máximo 3)</small></span><input type="file" accept="image/jpeg,image/png,image/webp" multiple onChange={(event) => setDispatchPhotos(Array.from(event.target.files ?? []).slice(0, 3))} />{dispatchPhotos.length ? <small>{dispatchPhotos.length} foto(s) seleccionada(s)</small> : null}</label><button type="submit" className="operations-primary-action" disabled={isDispatchSubmitting || bls.length === 0}>{isDispatchSubmitting ? 'Guardando despacho...' : 'Registrar despacho'}</button>{bls.length === 0 ? <p className="operations-hint">No hay BL activos para esta nave.</p> : null}</form></section>
        <section className="operations-column summary-column" aria-labelledby="summary-title"><div className="operations-column-heading"><span className="eyebrow">En vivo</span><h2 id="summary-title">Resumen dinámico</h2></div>{summaryError ? <p className="operations-message operations-error" role="alert">{summaryError}</p> : null}{isSummaryRefreshing ? <p className="operations-message">Actualizando resúmenes...</p> : null}{shipSummary ? <SummaryDashboard shipSummary={shipSummary} shiftSummary={summary} /> : !isLoading && !summaryError ? <p className="operations-message">No se pudo cargar el resumen de la nave.</p> : null}</section>
      </div>
    </main>
  )
}
