import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { getManagementInProcess, getManagementReport } from '../../services/managementReportService'
import type { ManagementInProcessReport, ManagementReport } from '../../types/managementReports'
import './ManagementReportsPage.css'

const monthNames = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre']

function formatQuantity(value: number) { return new Intl.NumberFormat('es-PE', { maximumFractionDigits: 0 }).format(value) }
function formatDateTime(value: string | null) {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? value : new Intl.DateTimeFormat('es-PE', { dateStyle: 'medium', timeStyle: 'short', timeZone: 'America/Lima' }).format(date)
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
  const [shipFilter, setShipFilter] = useState('all')
  const [report, setReport] = useState<ManagementReport | null>(null)
  const [inProcess, setInProcess] = useState<ManagementInProcessReport | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  const isAllowed = user?.role === 'Management' || user?.role === 'Administrator'

  useEffect(() => {
    if (!isAllowed) return
    let mounted = true
    setIsLoading(true); setError('')
    Promise.all([getManagementReport(year, month), getManagementInProcess()])
      .then(([monthly, active]) => { if (mounted) { setReport(monthly); setInProcess(active) } })
      .catch((requestError) => { if (mounted) setError(getErrorMessage(requestError)) })
      .finally(() => { if (mounted) setIsLoading(false) })
    return () => { mounted = false }
  }, [isAllowed, month, year])

  const shipOptions = useMemo(() => {
    const map = new Map<string, string>()
    report?.ships.forEach((ship) => map.set(ship.shipId, ship.shipName))
    inProcess?.ships.forEach((ship) => map.set(ship.shipId, ship.shipName))
    return Array.from(map.entries()).sort((a, b) => a[1].localeCompare(b[1]))
  }, [report, inProcess])

  const filteredShips = useMemo(() => report?.ships.filter((ship) => shipFilter === 'all' || ship.shipId === shipFilter) ?? [], [report, shipFilter])
  const filteredInProcess = useMemo(() => inProcess?.ships.filter((ship) => shipFilter === 'all' || ship.shipId === shipFilter) ?? [], [inProcess, shipFilter])

  const metrics = useMemo(() => {
    const totalDeclared = filteredShips.reduce((sum, ship) => sum + ship.declaredQuantity, 0)
    const totalReceived = filteredShips.reduce((sum, ship) => sum + ship.receivedQuantity, 0)
    const totalDispatched = filteredShips.reduce((sum, ship) => sum + ship.dispatchedQuantity, 0)
    const durations = filteredShips.filter((ship) => ship.calendarDurationHours > 0).map((ship) => ship.calendarDurationHours)
    return {
      completedShips: filteredShips.length,
      totalDeclared,
      totalReceived,
      totalDispatched,
      averageDuration: durations.length ? durations.reduce((sum, value) => sum + value, 0) / durations.length : 0,
      averageDispatch: filteredShips.length ? totalDispatched / filteredShips.length : 0,
      completionPercent: totalDeclared > 0 ? (totalDispatched / totalDeclared) * 100 : 0,
    }
  }, [filteredShips])

  const inProcessMetrics = useMemo(() => ({
    count: filteredInProcess.length,
    received: filteredInProcess.reduce((sum, ship) => sum + ship.receivedQuantity, 0),
    dispatched: filteredInProcess.reduce((sum, ship) => sum + ship.dispatchedQuantity, 0),
    available: filteredInProcess.reduce((sum, ship) => sum + ship.availableQuantity, 0),
  }), [filteredInProcess])

  function selectTrendMonth(selectedMonth: number) {
    setMonth(selectedMonth)
    window.setTimeout(() => document.getElementById('management-month-detail')?.scrollIntoView({ behavior: 'smooth', block: 'start' }), 50)
  }

  function exportCsv() {
    if (!report) return
    const rows: Array<Array<string | number>> = [
      ['REPORTE GERENCIAL IQBF'], ['Periodo', `${monthNames[report.month - 1]} ${report.year}`],
      ['Filtro nave', shipFilter === 'all' ? 'Todas' : shipOptions.find(([id]) => id === shipFilter)?.[1] ?? ''],
      ['Criterio de cierre', 'Stock cero y mes del ultimo despacho registrado de la nave'], [],
      ['Nave', 'Primera recepcion', 'Ultimo despacho', 'Declarado', 'Recibido', 'Despachado', 'Stock', 'BL', 'Tx recepcion', 'Tx despacho', 'Duracion calendario', 'Productos'],
      ...filteredShips.map((ship) => [ship.shipName, formatDateTime(ship.firstReceptionAt), formatDateTime(ship.lastDispatchAt), ship.declaredQuantity, ship.receivedQuantity, ship.dispatchedQuantity, ship.availableQuantity, ship.blCount, ship.receptionTransactions, ship.dispatchTransactions, formatDuration(ship.calendarDurationHours), ship.products.join(' / ')]),
      [], ['TOTAL', '', '', metrics.totalDeclared, metrics.totalReceived, metrics.totalDispatched], [],
      ['NAVES EN PROCESO'], ['Nave', 'Primera recepcion', 'Ultimo movimiento', 'Recibido', 'Despachado', 'Stock pendiente', 'Avance %', 'Dias transcurridos'],
      ...filteredInProcess.map((ship) => [ship.shipName, formatDateTime(ship.firstReceptionAt), formatDateTime(ship.lastMovementAt), ship.receivedQuantity, ship.dispatchedQuantity, ship.availableQuantity, ship.dispatchProgress, formatDuration(ship.calendarDurationHours)]),
    ]
    const csv = rows.map((row) => row.map((cell) => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(',')).join('\n')
    const blob = new Blob(['\uFEFF', csv], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob); const anchor = document.createElement('a')
    anchor.href = url; anchor.download = `IQBF_Gerencia_${report.year}_${String(report.month).padStart(2, '0')}.csv`
    document.body.appendChild(anchor); anchor.click(); anchor.remove(); URL.revokeObjectURL(url)
  }

  if (!isAllowed) return <main className="management-shell"><p>Acceso exclusivo para Gerencia y Administradores.</p></main>
  const maxTrend = Math.max(...(report?.monthlyTrend.map((item) => item.dispatchedQuantity) ?? [1]), 1)
  const selectedShipName = shipFilter === 'all' ? 'Todas las naves' : shipOptions.find(([id]) => id === shipFilter)?.[1] ?? 'Nave seleccionada'

  return (
    <main className="management-shell">
      <header className="management-header no-print">
        <div><span className="eyebrow">IQBF Control · Business Intelligence</span><h1>Panel Gerencial</h1><p>Indicadores ejecutivos de recepción y despacho por nave.</p></div>
        <div className="management-header-actions">
          {user?.role === 'Administrator' ? <button type="button" className="secondary-action" onClick={() => navigate('/ships')}>Ir a operación</button> : null}
          <button type="button" className="secondary-action" onClick={exportCsv} disabled={!report || isLoading}>Exportar CSV</button>
          <button type="button" className="secondary-action" onClick={() => window.print()} disabled={!report || isLoading}>Imprimir / PDF</button>
          <button type="button" className="secondary-action" onClick={logout}>Cerrar sesión</button>
        </div>
      </header>

      <section className="management-filter-card no-print">
        <div><span className="eyebrow">Filtros gerenciales</span><h2>Cierre mensual por último despacho</h2><p>Una nave cerrada se contabiliza en el mes de su último despacho. Las naves con stock pendiente aparecen en “En proceso”.</p></div>
        <div className="management-filters">
          <label>Año<input type="number" min="2020" max="2100" value={year} onChange={(event) => setYear(Number(event.target.value))} /></label>
          <label>Mes<select value={month} onChange={(event) => setMonth(Number(event.target.value))}>{monthNames.map((name, index) => <option key={name} value={index + 1}>{name}</option>)}</select></label>
          <label>Nave<select value={shipFilter} onChange={(event) => setShipFilter(event.target.value)}><option value="all">Todas las naves</option>{shipOptions.map(([id, name]) => <option key={id} value={id}>{name}</option>)}</select></label>
        </div>
      </section>

      {isLoading ? <p className="management-message">Actualizando indicadores gerenciales...</p> : null}
      {error ? <p className="management-message management-error">{error}</p> : null}

      {!isLoading && !error && report ? <>
        <section className="management-report-heading"><div><span>IQBF Control</span><h2>Resumen Gerencial · {monthNames[report.month - 1]} {report.year}</h2></div><div><strong>{selectedShipName}</strong><span>{metrics.completedShips} naves cerradas en el filtro</span></div></section>

        <section className="management-kpis">
          <article><span>Naves cerradas</span><strong>{metrics.completedShips}</strong><small>stock cero y último despacho en el mes</small></article>
          <article><span>Big Bags despachados</span><strong>{formatQuantity(metrics.totalDispatched)}</strong><small>acumulado del filtro</small></article>
          <article><span>Big Bags recibidos</span><strong>{formatQuantity(metrics.totalReceived)}</strong><small>acumulado del filtro</small></article>
          <article><span>Despacho / declarado</span><strong>{metrics.completionPercent.toFixed(1)}%</strong><small>{formatQuantity(metrics.totalDeclared)} BB declarados</small></article>
          <article><span>Promedio por nave</span><strong>{formatQuantity(Math.round(metrics.averageDispatch))}</strong><small>BB despachados</small></article>
          <article><span>Duración calendario promedio</span><strong>{formatDuration(metrics.averageDuration)}</strong><small>primera recepción → último despacho</small></article>
        </section>

        <section className="management-grid">
          <article className="management-card management-trend-card">
            <div className="management-card-heading"><div><span className="eyebrow">Tendencia anual interactiva</span><h2>Naves cerradas y volumen despachado</h2><p>Selecciona un mes para abrir su detalle.</p></div><strong>{year}</strong></div>
            <div className="management-trend">
              {report.monthlyTrend.map((item) => {
                const height = item.dispatchedQuantity > 0 ? Math.max(5, (item.dispatchedQuantity / maxTrend) * 100) : 0
                return <button type="button" className={`management-trend-item${month === item.month ? ' is-selected' : ''}`} key={item.month} title={`${monthNames[item.month - 1]}: ${item.completedShips} naves · ${formatQuantity(item.dispatchedQuantity)} BB`} onClick={() => selectTrendMonth(item.month)}><div className="management-trend-stage"><span style={{ height: `${height}%` }} /></div><strong>{item.completedShips}</strong><small>{monthNames[item.month - 1].slice(0, 3)}</small></button>
              })}
            </div>
            <div className="management-chart-note">Altura: volumen despachado · número: naves cerradas. Haz clic en una barra para ver ese mes.</div>
          </article>

          <article className="management-card">
            <div className="management-card-heading"><div><span className="eyebrow">Mix de carga</span><h2>{shipFilter === 'all' ? 'Resumen por producto' : 'Productos de la nave'}</h2></div></div>
            {shipFilter === 'all' ? <div className="management-table-wrap compact"><table><thead><tr><th>Producto</th><th>Naves</th><th>Declarado</th><th>Recibido</th><th>Despachado</th></tr></thead><tbody>{report.products.map((product) => <tr key={product.productName}><td><strong>{product.productName}</strong></td><td>{product.shipCount}</td><td>{formatQuantity(product.declaredQuantity)}</td><td>{formatQuantity(product.receivedQuantity)}</td><td>{formatQuantity(product.dispatchedQuantity)}</td></tr>)}</tbody></table></div> : <div className="management-product-chips">{[...new Set([...filteredShips.flatMap((ship) => ship.products), ...filteredInProcess.flatMap((ship) => ship.products)])].map((product) => <span key={product}>{product}</span>)}</div>}
          </article>
        </section>

        <section className="management-card management-process-card">
          <div className="management-card-heading"><div><span className="eyebrow">Seguimiento actual</span><h2>Naves en proceso</h2><p>Naves que ya recibieron carga y todavía mantienen stock pendiente de despacho.</p></div><span>{inProcessMetrics.count} en proceso</span></div>
          <div className="management-process-kpis"><div><span>Recibido</span><strong>{formatQuantity(inProcessMetrics.received)} BB</strong></div><div><span>Despachado</span><strong>{formatQuantity(inProcessMetrics.dispatched)} BB</strong></div><div><span>Stock pendiente</span><strong>{formatQuantity(inProcessMetrics.available)} BB</strong></div></div>
          {filteredInProcess.length === 0 ? <p className="management-empty">No hay naves en proceso para el filtro seleccionado.</p> : <div className="management-table-wrap"><table className="management-process-table"><thead><tr><th>Nave</th><th>Primera recepción</th><th>Última actividad</th><th>Productos</th><th>Recibido</th><th>Despachado</th><th>Stock pendiente</th><th>Avance</th><th>Tiempo transcurrido</th></tr></thead><tbody>{filteredInProcess.map((ship) => <tr key={ship.shipId}><td><strong>{ship.shipName}</strong></td><td>{formatDateTime(ship.firstReceptionAt)}</td><td>{formatDateTime(ship.lastMovementAt)}</td><td>{ship.products.join(', ') || '—'}</td><td>{formatQuantity(ship.receivedQuantity)}</td><td>{formatQuantity(ship.dispatchedQuantity)}</td><td><strong>{formatQuantity(ship.availableQuantity)}</strong></td><td><div className="management-progress"><span style={{ width: `${ship.dispatchProgress}%` }} /></div><small>{ship.dispatchProgress.toFixed(1)}%</small></td><td>{formatDuration(ship.calendarDurationHours)}</td></tr>)}</tbody></table></div>}
        </section>

        <section className="management-card management-ships-card" id="management-month-detail">
          <div className="management-card-heading"><div><span className="eyebrow">Cierre mensual</span><h2>Naves completadas · {monthNames[report.month - 1]}</h2><p>Si una nave comenzó el mes anterior pero su último despacho ocurrió en este periodo, se contabiliza aquí.</p></div><span>{filteredShips.length} naves</span></div>
          {filteredShips.length === 0 ? <p className="management-empty">No hay naves cerradas para este mes y filtro.</p> : <div className="management-table-wrap"><table><thead><tr><th>Nave</th><th>Primera recepción</th><th>Último despacho</th><th>Productos</th><th>BL</th><th>Recibido</th><th>Despachado</th><th>Stock</th><th>Tx despacho</th><th>Duración</th></tr></thead><tbody>{filteredShips.map((ship) => <tr key={ship.shipId}><td><strong>{ship.shipName}</strong></td><td>{formatDateTime(ship.firstReceptionAt)}</td><td><strong>{formatDateTime(ship.lastDispatchAt)}</strong></td><td>{ship.products.join(', ') || '—'}</td><td>{ship.blCount}</td><td>{formatQuantity(ship.receivedQuantity)}</td><td>{formatQuantity(ship.dispatchedQuantity)}</td><td>{formatQuantity(ship.availableQuantity)}</td><td>{ship.dispatchTransactions}</td><td>{formatDuration(ship.calendarDurationHours)}</td></tr>)}</tbody><tfoot><tr><td colSpan={5}>TOTAL DEL FILTRO</td><td>{formatQuantity(metrics.totalReceived)}</td><td>{formatQuantity(metrics.totalDispatched)}</td><td colSpan={3}></td></tr></tfoot></table></div>}
        </section>
      </> : null}
    </main>
  )
}
