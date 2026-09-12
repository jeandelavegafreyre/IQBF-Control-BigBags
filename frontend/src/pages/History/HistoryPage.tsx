import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useOperation } from '../../context/OperationContext'
import { getActiveBLsByShip } from '../../services/blService'
import { updateDispatch } from '../../services/dispatchService'
import { updateReception } from '../../services/receptionService'
import { getShiftMovements } from '../../services/reportService'
import type { BL } from '../../types/bls'
import type { OperationalMovement } from '../../types/reports'
import './HistoryPage.css'

type EditLine = { blId: string; quantity: string }
type EditState = {
  movement: OperationalMovement
  reference: string
  comment: string
  items: EditLine[]
}

function formatDateTime(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return new Intl.DateTimeFormat('es-PE', { dateStyle: 'short', timeStyle: 'medium' }).format(date)
}

function photoUrl(path: string): string {
  if (/^https?:\/\//i.test(path)) return path
  const apiUrl = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? ''
  return `${apiUrl}${path.startsWith('/') ? '' : '/'}${path}`
}

function movementLabel(movement: OperationalMovement): string {
  return movement.movementType === 'Reception' ? 'Recepción' : 'Despacho'
}

function getErrorMessage(error: unknown): string {
  if (typeof error === 'object' && error !== null) {
    const value = error as { response?: { data?: { error?: string; message?: string; title?: string } }; message?: string }
    return value.response?.data?.error || value.response?.data?.message || value.response?.data?.title || value.message || 'No se pudo guardar la corrección.'
  }
  return 'No se pudo guardar la corrección.'
}

export function HistoryPage() {
  const navigate = useNavigate()
  const { selectedShip, activeShift } = useOperation()
  const [movements, setMovements] = useState<OperationalMovement[]>([])
  const [bls, setBls] = useState<BL[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [filter, setFilter] = useState<'All' | 'Reception' | 'Dispatch'>('All')
  const [editing, setEditing] = useState<EditState | null>(null)
  const [isSaving, setIsSaving] = useState(false)

  async function loadHistory() {
    if (!activeShift) return
    setError('')
    try {
      const [history, activeBls] = await Promise.all([
        getShiftMovements(activeShift.id),
        getActiveBLsByShip(activeShift.shipId),
      ])
      setMovements(history)
      setBls(activeBls)
    } catch {
      setError('No se pudo cargar el historial operativo del turno.')
    }
  }

  useEffect(() => {
    if (!activeShift) return
    let mounted = true
    setIsLoading(true)
    Promise.all([getShiftMovements(activeShift.id), getActiveBLsByShip(activeShift.shipId)])
      .then(([history, activeBls]) => {
        if (!mounted) return
        setMovements(history)
        setBls(activeBls)
      })
      .catch(() => { if (mounted) setError('No se pudo cargar el historial operativo del turno.') })
      .finally(() => { if (mounted) setIsLoading(false) })
    return () => { mounted = false }
  }, [activeShift?.id, activeShift?.shipId])

  function startEdit(movement: OperationalMovement) {
    setError('')
    setSuccess('')
    setEditing({
      movement,
      reference: movement.reference,
      comment: movement.comment ?? '',
      items: movement.items.map((item) => ({ blId: item.blId, quantity: String(item.quantity) })),
    })
  }

  function updateEditLine(index: number, field: keyof EditLine, value: string) {
    setEditing((current) => current ? {
      ...current,
      items: current.items.map((item, itemIndex) => itemIndex === index ? { ...item, [field]: value } : item),
    } : current)
  }

  async function saveEdit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!editing || !activeShift) return

    const reference = editing.reference.trim()
    if (!reference) { setError('Completa el Terminal Truck o la placa.'); return }
    if (editing.movement.movementType === 'Reception' && !/^\d+$/.test(reference)) {
      setError('Terminal Truck debe contener únicamente números.')
      return
    }

    const items = editing.items.map((item) => ({ blId: item.blId, quantity: Number(item.quantity) }))
    if (items.some((item) => !item.blId || !Number.isInteger(item.quantity) || item.quantity <= 0)) {
      setError('Todos los BL y cantidades deben ser válidos. Las cantidades deben ser enteros mayores que cero.')
      return
    }
    if (new Set(items.map((item) => item.blId)).size !== items.length) {
      setError('No se puede repetir el mismo BL dentro de una transacción.')
      return
    }

    setIsSaving(true)
    setError('')
    try {
      if (editing.movement.movementType === 'Reception') {
        await updateReception(editing.movement.id, {
          shiftId: activeShift.id,
          terminalTruck: reference,
          comment: editing.comment || undefined,
          items,
        })
      } else {
        await updateDispatch(editing.movement.id, {
          shiftId: activeShift.id,
          plate: reference,
          comment: editing.comment || undefined,
          items,
        })
      }
      setEditing(null)
      setSuccess(`Transacción #${editing.movement.transactionNumber} corregida. La modificación quedó registrada con usuario, fecha y hora.`)
      await loadHistory()
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setIsSaving(false)
    }
  }

  if (!selectedShip || !activeShift) {
    return (
      <main className="history-shell">
        <p>No hay un turno activo seleccionado.</p>
        <button type="button" className="secondary-action" onClick={() => navigate('/ships')}>Volver a naves</button>
      </main>
    )
  }

  const visibleMovements = filter === 'All' ? movements : movements.filter((movement) => movement.movementType === filter)

  return (
    <main className="history-shell">
      <header className="history-header">
        <div>
          <span className="eyebrow">IQBF Control · Trazabilidad</span>
          <h1>Transacciones del turno</h1>
          <p>{selectedShip.name} · {activeShift.shiftDate} · más recientes primero</p>
        </div>
        <button type="button" className="secondary-action" onClick={() => navigate('/operations')}>Volver a operación</button>
      </header>

      <section className="history-toolbar" aria-label="Filtros de historial">
        <button type="button" className={filter === 'All' ? 'is-active' : ''} onClick={() => setFilter('All')}>Todos ({movements.length})</button>
        <button type="button" className={filter === 'Reception' ? 'is-active' : ''} onClick={() => setFilter('Reception')}>Recepciones ({movements.filter((x) => x.movementType === 'Reception').length})</button>
        <button type="button" className={filter === 'Dispatch' ? 'is-active' : ''} onClick={() => setFilter('Dispatch')}>Despachos ({movements.filter((x) => x.movementType === 'Dispatch').length})</button>
      </section>

      {isLoading ? <p className="history-status">Cargando historial...</p> : null}
      {error ? <p className="history-status history-error" role="alert">{error}</p> : null}
      {success ? <p className="history-status history-success" role="status">{success}</p> : null}
      {!isLoading && !error && visibleMovements.length === 0 ? <p className="history-status">No hay movimientos para mostrar.</p> : null}

      <section className="history-list">
        {visibleMovements.map((movement) => (
          <article className={`history-card ${movement.updatedAt ? 'is-edited' : ''}`} key={`${movement.movementType}-${movement.id}`}>
            <div className="history-card-heading">
              <div>
                <span className={`history-kind ${movement.movementType === 'Reception' ? 'reception' : 'dispatch'}`}>{movementLabel(movement)}</span>
                <h2>{movementLabel(movement)} #{movement.transactionNumber}</h2>
              </div>
              <div className="history-card-actions">
                <time dateTime={movement.createdAt}>{formatDateTime(movement.createdAt)}</time>
                <button type="button" className="history-edit-button" onClick={() => startEdit(movement)} aria-label={`Editar ${movementLabel(movement)} ${movement.transactionNumber}`} title="Editar transacción">
                  <span aria-hidden="true">✎</span> Editar
                </button>
              </div>
            </div>

            <div className="history-meta">
              <div><span>{movement.movementType === 'Reception' ? 'Terminal Truck' : 'Placa'}</span><strong>{movement.reference}</strong></div>
              <div><span>Registrado por</span><strong>{movement.createdBy || 'Sin identificar'}</strong></div>
              <div><span>Fecha y hora</span><strong>{formatDateTime(movement.createdAt)}</strong></div>
              {movement.updatedAt ? <div className="history-audit"><span>Modificado</span><strong>{movement.updatedBy || 'Sin identificar'} · {formatDateTime(movement.updatedAt)}</strong></div> : null}
              {movement.comment ? <div className="history-comment"><span>Comentario</span><strong>{movement.comment}</strong></div> : null}
            </div>

            <div className="history-items">
              {movement.items.map((item) => (
                <div className="history-item" key={`${movement.id}-${item.blId}`}>
                  <div><strong>{item.blCode}</strong><span>{item.productName}</span></div>
                  <strong>{item.quantity} Big Bag{item.quantity === 1 ? '' : 's'}</strong>
                </div>
              ))}
            </div>

            {movement.photos.length > 0 ? (
              <div className="history-photos">
                {movement.photos.map((photo) => (
                  <a key={photo.id} href={photoUrl(photo.photoUrl)} target="_blank" rel="noreferrer" title={photo.fileName ?? 'Evidencia fotográfica'}>
                    <img src={photoUrl(photo.photoUrl)} alt={`Evidencia de ${movementLabel(movement).toLowerCase()} ${movement.transactionNumber}`} loading="lazy" />
                  </a>
                ))}
              </div>
            ) : <p className="history-no-photo">Sin evidencia fotográfica.</p>}
          </article>
        ))}
      </section>

      {editing ? (
        <div className="history-edit-backdrop" role="presentation" onMouseDown={() => !isSaving && setEditing(null)}>
          <form className="history-edit-dialog" onSubmit={saveEdit} onMouseDown={(event) => event.stopPropagation()}>
            <div className="history-edit-heading">
              <div><span className="eyebrow">Corrección auditada</span><h2>Editar {movementLabel(editing.movement)} #{editing.movement.transactionNumber}</h2></div>
              <button type="button" className="history-dialog-close" onClick={() => setEditing(null)} disabled={isSaving} aria-label="Cerrar">×</button>
            </div>

            <label className="history-edit-field">
              <span>{editing.movement.movementType === 'Reception' ? 'Terminal Truck' : 'Placa'}</span>
              <input value={editing.reference} onChange={(event) => setEditing((current) => current ? { ...current, reference: event.target.value } : current)} required />
            </label>

            <div className="history-edit-items">
              {editing.items.map((item, index) => (
                <div className="history-edit-row" key={index}>
                  <label className="history-edit-field"><span>BL</span><select value={item.blId} onChange={(event) => updateEditLine(index, 'blId', event.target.value)} required>
                    {bls.map((bl) => <option key={bl.id} value={bl.id}>{bl.code} · {bl.productName}</option>)}
                  </select></label>
                  <label className="history-edit-field"><span>Cantidad</span><input type="number" min="1" step="1" value={item.quantity} onChange={(event) => updateEditLine(index, 'quantity', event.target.value)} required /></label>
                </div>
              ))}
            </div>

            <label className="history-edit-field"><span>Comentario</span><textarea rows={3} maxLength={100} value={editing.comment} onChange={(event) => setEditing((current) => current ? { ...current, comment: event.target.value } : current)} /></label>
            <p className="history-edit-note">Al guardar, el registro conservará su fecha/usuario original y mostrará además quién realizó la modificación y la fecha/hora de corrección.</p>
            <div className="history-edit-footer">
              <button type="button" className="secondary-action" onClick={() => setEditing(null)} disabled={isSaving}>Cancelar</button>
              <button type="submit" className="primary-action" disabled={isSaving}>{isSaving ? 'Guardando...' : 'Guardar corrección'}</button>
            </div>
          </form>
        </div>
      ) : null}
    </main>
  )
}
