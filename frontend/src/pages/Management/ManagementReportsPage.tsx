import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { getManagementReport } from '../../services/managementReportService'
import type { ManagementReport } from '../../types/managementReports'
import './ManagementReportsPage.css'

const monthNames = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre']

function formatQuantity(value: number) {
  return new Intl.NumberFormat('es-PE', { maximumFractionDigits: 0 }).format(value)
}

function formatDateTime(value: string | null) {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return new Intl.DateTimeFormat('es-PE', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'America/Lima',
  }).format(date)
}

function formatDuration(hours: number) {
  if (!hours) return '—'
  if (hours < 24) return `${hours.toFixed(1)} h`
  return `${(hours / 24).toFixed(1)} días`
}

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null) {
    const candidate = error as { response?: { data?: { error?: string; message?: string } }; message?: string }
    if (candidate.response?.data?.error) return candidate.response.data.error
    if (candidate.response?.data?.message) return candidate.response.data.message
    if (candidate.message) return candidate.message
  }
  return 'No se pudo cargar el reporte gerencial.'
}

export function ManagementReportsPage() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const today = new Date()
  const [year, setYear] = useState(today.getFullYear())
  const [month, setMonth] = useState(today.getMonth() + 1)
  const [report, setReport] = useState<ManagementReport | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  const isAllowed = user?.role === 'Management' || user?.role === 'Administrator'

  useEffect(() => {
    if (!isAllowed) return
    let mounted = true
    setIsLoading(true)
    setError('')
    getManagementReport(year, month)
      .then((data) => { if (mounted) setReport(data) })
      .catch((requestError) => { if (mounted) setError(getErrorMessage(requestError)) })
      .finally(() => { if (mounted) setIsLoading(false) })
    return () => { mounted = false }
  }, [isAllowed, month, year])

  const metrics = useMemo(() => {
    if (!report) return { averageDuration: 0, averageDispatch: 0, completionPercent: 0 }
    const durations = report.ships.filter((ship) => ship.calendarDurationHours > 0).map((ship) => ship.calendarDurationHours)
    return {
      averageDuration: durations.length ? durations.reduce((sum, value) => sum + value, 0) / durations.length : 0,
      averageDispatch: report.completedShips ? report.totalDispatched / report.completedShips : 0,
      completionPercent: report.totalDeclared > 0 ? (report.totalDispatched / report.totalDeclared) * 100 : 0,
    }
  }, [report])

  function exportCsv() {
    if (!report) return
    const rows: Array<Array<string | number>> = [
      ['REPORTE GERENCIAL IQBF'],
      ['Periodo', `${monthNames[report.month - 1]} ${report.year}`],
      ['Criterio de cierre', 'Mes del ultimo despacho registrado de la nave'],
      [],
      ['Nave', 'Primera recepcion', 'Ultimo despacho', 'Declarado', 'Recibido', 'Despachado', 'Stock', 'BL', 'Tx recepcion', 'Tx despacho', 'Duracion calendario', 'Productos'],
      ...report.ships.map((ship) => [
        ship.shipName,
        formatDateTime(ship.firstReceptionAt),
        formatDateTime(ship.lastDispatchAt),
        ship.declaredQuantity,
        ship.receivedQuantity,
        ship.dispatchedQuantity,
        ship.availableQuantity,
        ship.blCount,
        ship.receptionTransactions,
        ship.dispatchTransactions,
        formatDuration(ship.calendarDurationHours),
        ship.products.join(' / '),
      ]),
      [],
      ['TOTAL', '', '', report.totalDeclared, report.totalReceived, report.totalDispatched],
    ]
    const csv = rows.map((row) => row.map((cell) => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(',')).join('\n')
    const blob = new Blob(['\uFEFF', csv], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `IQBF_Gerencia_${report.year}_${String(report.month).padStart(2, '0')}.csv`
    document.body.appendChild(anchor)
    anchor.click()
    anchor.remove()
    URL.revokeObjectURL(url)
  }

  if (!isAllowed) {
    return <main className="management-shell"><p>Acceso exclusivo para Gerencia y Administradores.</p></main>
  }

  const maxTrend = Math.max(...(report?.monthlyTrend.map((item) => item.dispatchedQuantity) ?? [1]), 1)

  return (
    <main className="management-shell">
      <header className="management-header no-print">
        <div>
          <span className="eyebrow">IQBF Control · Business Intelligence</span>
          <h1>Panel Gerencial</h1>
          <p>Indicadores ejecutivos de recepción y despacho por nave.</p>
        </div>
        <div className="management-header-actions">
          {user?.role === 'Administrator' ? <button type="button" className="secondary-action" onClick={() => navigate('/ships')}>Ir a operación</button> : null}
          <button type="button" className="secondary-action" onClick={exportCsv} disabled={!report || isLoading}>Exportar CSV</button>
          <button type="button" className="secondary-action" onClick={() => window.print()} disabled={!report || isLoading}>Imprimir / PDF</button>
          <button type="button" className="secondary-action" onClick={logout}>Cerrar sesión</button>
        </div>
      </header>

      <section className="management-filter-card no-print">
        <div><span className="eyebrow">Periodo de análisis</span><h2>Cierre mensual por último despacho</h2><p>Una nave se contabiliza en el mes donde ocurrió su último despacho registrado, sin importar cuándo inició recepción o despacho.</p></div>
        <div className="management-filters">
          <label>Año<input type="number" min="2020" max="2100" value={year} onChange={(event) => setYear(Number(event.target.value))} /></label>
          <label>Mes<select value={month} onChange={(event) => setMonth(Number(event.target.value))}>{monthNames.map((name, index) => <option key={name} value={index + 1}>{name}</option>)}</select></label>
        </div>
      </section>

      {isLoading ? <p className="management-message">Actualizando indicadores gerenciales...</p> : null}
      {error ? <p className="management-message management-error">{error}</p> : null}

      {!isLoading && !error && report ? (
        <>
          <section className="management-report-heading">
            <div><span>IQBF Control</span><h2>Resumen Gerencial · {monthNames[report.month - 1]} {report.year}</h2></div>
            <div><strong>{report.completedShips} naves cerradas</strong><span>Clasificadas por último despacho · hora Perú</span></div>
          </section>

          <section className="management-kpis">
            <article><span>Naves cerradas</span><strong>{report.completedShips}</strong><small>último despacho en el mes</small></article>
            <article><span>Big Bags despachados</span><strong>{formatQuantity(report.totalDispatched)}</strong><small>acumulado de las naves cerradas</small></article>
            <article><span>Big Bags recibidos</span><strong>{formatQuantity(report.totalReceived)}</strong><small>acumulado de las naves cerradas</small></article>
            <article><span>Despacho / declarado</span><strong>{metrics.completionPercent.toFixed(1)}%</strong><small>{formatQuantity(report.totalDeclared)} BB declarados</small></article>
            <article><span>Promedio por nave</span><strong>{formatQuantity(Math.round(metrics.averageDispatch))}</strong><small>BB despachados</small></article>
            <article><span>Duración calendario promedio</span><strong>{formatDuration(metrics.averageDuration)}</strong><small>primera recepción → último despacho</small></article>
          </section>

          <section className="management-grid">
            <article className="management-card management-trend-card">
              <div className="management-card-heading"><div><span className="eyebrow">Tendencia anual</span><h2>Naves cerradas y volumen despachado</h2></div><strong>{year}</strong></div>
              <div className="management-trend">
                {report.monthlyTrend.map((item) => {
                  const height = item.dispatchedQuantity > 0 ? Math.max(5, (item.dispatchedQuantity / maxTrend) * 100) : 0
                  return <div className="management-trend-item" key={item.month} title={`${monthNames[item.month - 1]}: ${item.completedShips} naves · ${formatQuantity(item.dispatchedQuantity)} BB`}><div className="management-trend-stage"><span style={{ height: `${height}%` }} /></div><strong>{item.completedShips}</strong><small>{monthNames[item.month - 1].slice(0, 3)}</small></div>
                })}
              </div>
              <div className="management-chart-note">Altura de barra: volumen despachado · número superior: naves cuyo último despacho ocurrió en ese mes.</div>
            </article>

            <article className="management-card">
              <div className="management-card-heading"><div><span className="eyebrow">Mix de carga</span><h2>Resumen por producto</h2></div><span>{report.products.length} productos</span></div>
              <div className="management-table-wrap compact">
                <table><thead><tr><th>Producto</th><th>Naves</th><th>Declarado</th><th>Recibido</th><th>Despachado</th></tr></thead><tbody>{report.products.map((product) => <tr key={product.productName}><td><strong>{product.productName}</strong></td><td>{product.shipCount}</td><td>{formatQuantity(product.declaredQuantity)}</td><td>{formatQuantity(product.receivedQuantity)}</td><td>{formatQuantity(product.dispatchedQuantity)}</td></tr>)}</tbody></table>
              </div>
            </article>
          </section>

          <section className="management-card management-ships-card">
            <div className="management-card-heading"><div><span className="eyebrow">Cierre mensual</span><h2>Naves completadas por último despacho</h2><p>Si una nave comenzó el mes anterior pero su último despacho ocurrió en este periodo, se contabiliza aquí.</p></div><span>{report.ships.length} naves</span></div>
            {report.ships.length === 0 ? <p className="management-empty">No hay naves cuyo último despacho corresponda a este mes.</p> : (
              <div className="management-table-wrap">
                <table>
                  <thead><tr><th>Nave</th><th>Primera recepción</th><th>Último despacho</th><th>Productos</th><th>BL</th><th>Recibido</th><th>Despachado</th><th>Stock</th><th>Tx despacho</th><th>Duración</th></tr></thead>
                  <tbody>{report.ships.map((ship) => <tr key={ship.shipId}><td><strong>{ship.shipName}</strong></td><td>{formatDateTime(ship.firstReceptionAt)}</td><td><strong>{formatDateTime(ship.lastDispatchAt)}</strong></td><td>{ship.products.join(', ') || '—'}</td><td>{ship.blCount}</td><td>{formatQuantity(ship.receivedQuantity)}</td><td>{formatQuantity(ship.dispatchedQuantity)}</td><td>{formatQuantity(ship.availableQuantity)}</td><td>{ship.dispatchTransactions}</td><td>{formatDuration(ship.calendarDurationHours)}</td></tr>)}</tbody>
                  <tfoot><tr><td colSpan={5}>TOTAL DEL MES</td><td>{formatQuantity(report.totalReceived)}</td><td>{formatQuantity(report.totalDispatched)}</td><td colSpan={3}></td></tr></tfoot>
                </table>
              </div>
            )}
          </section>
        </>
      ) : null}
    </main>
  )
}
