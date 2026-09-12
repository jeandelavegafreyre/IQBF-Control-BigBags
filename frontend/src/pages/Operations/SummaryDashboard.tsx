import type { ShipSummary, ShiftSummary } from '../../types/dashboard'
import './SummaryDashboard.css'

interface SummaryDashboardProps {
  shipSummary: ShipSummary
  shiftSummary: ShiftSummary | null
}

function formatQuantity(quantity: number): string {
  return new Intl.NumberFormat('es-ES', { minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(quantity)
}

function clampPercent(value: number): number {
  return Math.max(0, Math.min(100, Number.isFinite(value) ? value : 0))
}

export function SummaryDashboard({ shipSummary, shiftSummary }: SummaryDashboardProps) {
  const maxDeclared = Math.max(...shipSummary.bLs.map((bl) => bl.totalQuantity), 1)
  const receivedPercent = clampPercent(shipSummary.receptionProgress)
  const dispatchedPercent = shipSummary.receivedQuantity > 0
    ? clampPercent((shipSummary.dispatchedQuantity / shipSummary.receivedQuantity) * 100)
    : 0

  return (
    <div className="live-summary-dashboard">
      <div className="live-summary-kpis">
        <article className="live-kpi live-kpi-received">
          <div className="live-kpi-label"><span aria-hidden="true">↓</span> Recibido</div>
          <strong>{formatQuantity(shipSummary.receivedQuantity)}</strong>
          <div className="live-progress" aria-label={`${receivedPercent.toFixed(0)}% recibido del lote`}>
            <span style={{ width: `${receivedPercent}%` }} />
          </div>
          <small>{receivedPercent.toFixed(0)}% del lote</small>
        </article>

        <article className="live-kpi live-kpi-dispatched">
          <div className="live-kpi-label"><span aria-hidden="true">↑</span> Despachado</div>
          <strong>{formatQuantity(shipSummary.dispatchedQuantity)}</strong>
          <div className="live-progress" aria-label={`${dispatchedPercent.toFixed(0)}% despachado de lo recibido`}>
            <span style={{ width: `${dispatchedPercent}%` }} />
          </div>
          <small>{dispatchedPercent.toFixed(0)}% de lo recibido</small>
        </article>
      </div>

      <div className="live-summary-strip stock-strip">
        <span>Stock neto en patio</span>
        <strong>{formatQuantity(shipSummary.availableQuantity)} u</strong>
      </div>
      <div className="live-summary-strip lot-strip">
        <span>Lote total nave</span>
        <strong>{formatQuantity(shipSummary.totalQuantity)} u</strong>
      </div>

      <section className="live-chart-card" aria-labelledby="chart-title">
        <div className="live-card-heading">
          <div>
            <span className="eyebrow">Comparativo acumulado</span>
            <h3 id="chart-title">Gráfico por BL</h3>
          </div>
          <span className="live-badge">En vivo</span>
        </div>

        {shipSummary.bLs.length === 0 ? (
          <p className="operations-hint">No hay BL registrados para esta nave.</p>
        ) : (
          <>
            <div className="bl-bar-chart" role="img" aria-label="Recepción y despacho acumulado por BL">
              {shipSummary.bLs.map((bl) => {
                const receivedHeight = clampPercent((bl.receivedQuantity / maxDeclared) * 100)
                const dispatchedHeight = clampPercent((bl.dispatchedQuantity / maxDeclared) * 100)
                const lotHeight = clampPercent((bl.totalQuantity / maxDeclared) * 100)
                return (
                  <div className="bl-bar-group" key={bl.id}>
                    <div className="bl-bar-stage">
                      <span className="bl-lot-marker" style={{ bottom: `${lotHeight}%` }} title={`Lote: ${formatQuantity(bl.totalQuantity)}`} />
                      <div className="bl-bar bl-bar-received" style={{ height: `${receivedHeight}%` }} title={`Recibido: ${formatQuantity(bl.receivedQuantity)}`}>
                        <span>{formatQuantity(bl.receivedQuantity)}</span>
                      </div>
                      <div className="bl-bar bl-bar-dispatched" style={{ height: `${dispatchedHeight}%` }} title={`Despachado: ${formatQuantity(bl.dispatchedQuantity)}`}>
                        <span>{formatQuantity(bl.dispatchedQuantity)}</span>
                      </div>
                    </div>
                    <strong title={bl.code}>{bl.code}</strong>
                    <small title={bl.productName}>{bl.productName}</small>
                  </div>
                )
              })}
            </div>
            <div className="chart-legend" aria-hidden="true">
              <span><i className="legend-received" /> Recepción</span>
              <span><i className="legend-dispatched" /> Despacho</span>
              <span><i className="legend-lot" /> Lote</span>
            </div>
          </>
        )}
      </section>

      <section className="live-bl-table" aria-labelledby="bl-detail-title">
        <div className="live-card-heading">
          <div>
            <span className="eyebrow">Control por documento</span>
            <h3 id="bl-detail-title">Detalle acumulado por BL</h3>
          </div>
        </div>
        <div className="live-table-scroll">
          <table>
            <thead>
              <tr><th>BL</th><th>Lote</th><th>Rec.</th><th>Desp.</th><th>Stock</th></tr>
            </thead>
            <tbody>
              {shipSummary.bLs.map((bl) => (
                <tr key={bl.id}>
                  <td><strong>{bl.code}</strong><small>{bl.productName}</small></td>
                  <td>{formatQuantity(bl.totalQuantity)}</td>
                  <td className="cell-received">{formatQuantity(bl.receivedQuantity)}</td>
                  <td className="cell-dispatched">{formatQuantity(bl.dispatchedQuantity)}</td>
                  <td className="cell-stock">{formatQuantity(bl.availableQuantity)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="live-shift-card" aria-labelledby="shift-live-title">
        <div className="live-card-heading">
          <div>
            <span className="eyebrow">Movimiento del período</span>
            <h3 id="shift-live-title">Turno actual</h3>
          </div>
        </div>
        {shiftSummary ? (
          <div className="live-shift-metrics">
            <div><span>Recibido</span><strong>{formatQuantity(shiftSummary.receivedQuantity)}</strong></div>
            <div><span>Despachado</span><strong>{formatQuantity(shiftSummary.dispatchedQuantity)}</strong></div>
            <div><span>Stock neto turno</span><strong>{formatQuantity(shiftSummary.netQuantity)}</strong></div>
          </div>
        ) : <p className="operations-hint">No se pudo cargar el movimiento del turno.</p>}
      </section>
    </div>
  )
}
