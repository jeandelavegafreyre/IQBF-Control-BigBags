import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useOperation } from '../../context/OperationContext'
import { getShiftMovements } from '../../services/reportService'
import type { OperationalMovement } from '../../types/reports'
import './ReportsPage.css'

type BLReportRow = {
  blId: string
  blCode: string
  productName: string
  received: number
  dispatched: number
  balance: number
}

function formatQuantity(value: number) {
  return new Intl.NumberFormat('es-PE', { maximumFractionDigits: 0 }).format(value)
}

function formatDate(value: string) {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString('es-PE', { dateStyle: 'short', timeStyle: 'short' })
}

function shiftLabel(shiftType: number) {
  return shiftType === 1 ? 'Día · 06:00–18:00' : 'Noche · 18:00–06:00'
}

function errorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null) {
    const candidate = error as { response?: { data?: { error?: string; message?: string } }; message?: string }
    if (candidate.response?.data?.error) return candidate.response.data.error
    if (candidate.response?.data?.message) return candidate.response.data.message
    if (candidate.message) return candidate.message
  }
  return 'No se pudo cargar el reporte.'
}

export function ReportsPage() {
  const navigate = useNavigate()
  const { selectedShip, activeShift } = useOperation()
  const [movements, setMovements] = useState<OperationalMovement[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!activeShift) return
    let mounted = true
    setIsLoading(true)
    setError('')
    getShiftMovements(activeShift.id)
      .then((data) => { if (mounted) setMovements(data) })
      .catch((requestError) => { if (mounted) setError(errorMessage(requestError)) })
      .finally(() => { if (mounted) setIsLoading(false) })
    return () => { mounted = false }
  }, [activeShift?.id])

  const report = useMemo(() => {
    const byBL = new Map<string, BLReportRow>()
    let received = 0
    let dispatched = 0
    let receptionTransactions = 0
    let dispatchTransactions = 0

    for (const movement of movements) {
      const isReception = movement.movementType === 'Reception'
      if (isReception) receptionTransactions += 1
      else dispatchTransactions += 1

      for (const item of movement.items) {
        const current = byBL.get(item.blId) ?? {
          blId: item.blId,
          blCode: item.blCode,
          productName: item.productName,
          received: 0,
          dispatched: 0,
          balance: 0,
        }
        if (isReception) {
          current.received += item.quantity
          received += item.quantity
        } else {
          current.dispatched += item.quantity
          dispatched += item.quantity
        }
        current.balance = current.received - current.dispatched
        byBL.set(item.blId, current)
      }
    }

    return {
      received,
      dispatched,
      balance: received - dispatched,
      receptionTransactions,
      dispatchTransactions,
      rows: Array.from(byBL.values()).sort((a, b) => a.blCode.localeCompare(b.blCode)),
    }
  }, [movements])

  function exportCsv() {
    if (!activeShift) return
    const rows = [
      ['Nave', selectedShip?.name ?? activeShift.shipName],
      ['Fecha', activeShift.shiftDate],
      ['Turno', shiftLabel(activeShift.shiftType)],
      [],
      ['BL', 'Producto', 'Recepcion', 'Despacho', 'Saldo'],
      ...report.rows.map((row) => [row.blCode, row.productName, row.received, row.dispatched, row.balance]),
      [],
      ['TOTAL', '', report.received, report.dispatched, report.balance],
    ]
    const csv = rows
      .map((row) => row.map((cell) => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(','))
      .join('\n')
    const blob = new Blob(['\uFEFF', csv], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `IQBF_${(selectedShip?.name ?? activeShift.shipName).replace(/\s+/g, '_')}_${activeShift.shiftDate}.csv`
    document.body.appendChild(anchor)
    anchor.click()
    anchor.remove()
    URL.revokeObjectURL(url)
  }

  if (!activeShift) return null

  return (
    <main className="reports-shell">
      <header className="reports-header no-print">
        <div>
          <span className="eyebrow">IQBF Control</span>
          <h1>Reportes operativos</h1>
          <p>{selectedShip?.name ?? activeShift.shipName} · {activeShift.shiftDate} · {shiftLabel(activeShift.shiftType)}</p>
        </div>
        <div className="reports-actions">
          <button type="button" className="secondary-action" onClick={() => navigate('/operations')}>Volver a operación</button>
          <button type="button" className="secondary-action" onClick={exportCsv} disabled={isLoading || movements.length === 0}>Exportar CSV</button>
          <button type="button" className="reports-primary" onClick={() => window.print()} disabled={isLoading}>Imprimir / PDF</button>
        </div>
      </header>

      <section className="reports-document">
        <div className="reports-print-heading">
          <div><span>IQBF Control</span><h2>Reporte operativo de turno</h2></div>
          <div className="reports-print-meta"><strong>{selectedShip?.name ?? activeShift.shipName}</strong><span>{activeShift.shiftDate} · {shiftLabel(activeShift.shiftType)}</span></div>
        </div>

        {isLoading ? <p className="reports-message">Generando reporte...</p> : null}
        {error ? <p className="reports-message reports-error">{error}</p> : null}

        {!isLoading && !error ? (
          <>
            <div className="reports-kpis">
              <article><span>Recepción</span><strong>{formatQuantity(report.received)}</strong><small>Big Bags</small></article>
              <article><span>Despacho</span><strong>{formatQuantity(report.dispatched)}</strong><small>Big Bags</small></article>
              <article><span>Saldo</span><strong>{formatQuantity(report.balance)}</strong><small>Big Bags</small></article>
              <article><span>Movimientos</span><strong>{movements.length}</strong><small>{report.receptionTransactions} recepción · {report.dispatchTransactions} despacho</small></article>
            </div>

            <section className="reports-section">
              <div className="reports-section-title"><div><span className="eyebrow">Consolidado</span><h2>Balance por BL</h2></div><span>{report.rows.length} BL</span></div>
              {report.rows.length === 0 ? <p className="reports-empty">No hay movimientos registrados en este turno.</p> : (
                <div className="reports-table-wrap">
                  <table className="reports-table">
                    <thead><tr><th>BL</th><th>Producto</th><th>Recepción</th><th>Despacho</th><th>Saldo</th></tr></thead>
                    <tbody>{report.rows.map((row) => <tr key={row.blId}><td><strong>{row.blCode}</strong></td><td>{row.productName}</td><td>{formatQuantity(row.received)}</td><td>{formatQuantity(row.dispatched)}</td><td><strong>{formatQuantity(row.balance)}</strong></td></tr>)}</tbody>
                    <tfoot><tr><td colSpan={2}>TOTAL</td><td>{formatQuantity(report.received)}</td><td>{formatQuantity(report.dispatched)}</td><td>{formatQuantity(report.balance)}</td></tr></tfoot>
                  </table>
                </div>
              )}
            </section>

            <section className="reports-section">
              <div className="reports-section-title"><div><span className="eyebrow">Detalle</span><h2>Movimientos del turno</h2></div><span>{movements.length} registros</span></div>
              {movements.length === 0 ? <p className="reports-empty">No hay movimientos para mostrar.</p> : (
                <div className="reports-movements">
                  {movements.map((movement) => (
                    <article className="reports-movement" key={movement.id}>
                      <div className="reports-movement-head">
                        <div><span className={`reports-movement-type ${movement.movementType === 'Reception' ? 'is-reception' : 'is-dispatch'}`}>{movement.movementType === 'Reception' ? 'Recepción' : 'Despacho'}</span><strong>#{movement.transactionNumber}</strong></div>
                        <span>{formatDate(movement.createdAt)}</span>
                      </div>
                      <div className="reports-movement-meta"><span>{movement.movementType === 'Reception' ? 'Terminal Truck' : 'Placa'}: <strong>{movement.reference}</strong></span><span>Operador: <strong>{movement.createdBy ?? '—'}</strong></span>{movement.updatedAt ? <span>Corregido: <strong>{movement.updatedBy ?? '—'}</strong></span> : null}</div>
                      <div className="reports-movement-items">{movement.items.map((item) => <div key={`${movement.id}-${item.blId}`}><span>{item.blCode} · {item.productName}</span><strong>{formatQuantity(item.quantity)} BB</strong></div>)}</div>
                      {movement.comment ? <p className="reports-comment">{movement.comment}</p> : null}
                    </article>
                  ))}
                </div>
              )}
            </section>
          </>
        ) : null}
      </section>
    </main>
  )
}
