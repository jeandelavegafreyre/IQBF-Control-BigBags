import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useOperation } from '../../context/OperationContext'
import { getShiftMovements } from '../../services/reportService'
import type { OperationalMovement } from '../../types/reports'
import './HistoryPage.css'

function formatDateTime(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return new Intl.DateTimeFormat('es-PE', {
    dateStyle: 'short',
    timeStyle: 'medium',
  }).format(date)
}

function photoUrl(path: string): string {
  if (/^https?:\/\//i.test(path)) return path
  const apiUrl = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? ''
  return `${apiUrl}${path.startsWith('/') ? '' : '/'}${path}`
}

function movementLabel(movement: OperationalMovement): string {
  return movement.movementType === 'Reception' ? 'Recepción' : 'Despacho'
}

export function HistoryPage() {
  const navigate = useNavigate()
  const { selectedShip, activeShift } = useOperation()
  const [movements, setMovements] = useState<OperationalMovement[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState<'All' | 'Reception' | 'Dispatch'>('All')

  useEffect(() => {
    if (!activeShift) return
    let mounted = true
    setIsLoading(true)
    setError('')

    getShiftMovements(activeShift.id)
      .then((data) => {
        if (mounted) setMovements(data)
      })
      .catch(() => {
        if (mounted) setError('No se pudo cargar el historial operativo del turno.')
      })
      .finally(() => {
        if (mounted) setIsLoading(false)
      })

    return () => { mounted = false }
  }, [activeShift?.id])

  if (!selectedShip || !activeShift) {
    return (
      <main className="history-shell">
        <p>No hay un turno activo seleccionado.</p>
        <button type="button" className="secondary-action" onClick={() => navigate('/ships')}>Volver a naves</button>
      </main>
    )
  }

  const visibleMovements = filter === 'All'
    ? movements
    : movements.filter((movement) => movement.movementType === filter)

  return (
    <main className="history-shell">
      <header className="history-header">
        <div>
          <span className="eyebrow">IQBF Control · Trazabilidad</span>
          <h1>Historial operativo</h1>
          <p>{selectedShip.name} · {activeShift.shiftDate}</p>
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
      {!isLoading && !error && visibleMovements.length === 0 ? <p className="history-status">No hay movimientos para mostrar.</p> : null}

      <section className="history-list">
        {visibleMovements.map((movement) => (
          <article className="history-card" key={`${movement.movementType}-${movement.id}`}>
            <div className="history-card-heading">
              <div>
                <span className={`history-kind ${movement.movementType === 'Reception' ? 'reception' : 'dispatch'}`}>
                  {movementLabel(movement)}
                </span>
                <h2>{movementLabel(movement)} #{movement.transactionNumber}</h2>
              </div>
              <time dateTime={movement.createdAt}>{formatDateTime(movement.createdAt)}</time>
            </div>

            <div className="history-meta">
              <div><span>{movement.movementType === 'Reception' ? 'Terminal Truck' : 'Placa'}</span><strong>{movement.reference}</strong></div>
              <div><span>Usuario</span><strong>{movement.createdBy || 'Sin identificar'}</strong></div>
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
    </main>
  )
}
