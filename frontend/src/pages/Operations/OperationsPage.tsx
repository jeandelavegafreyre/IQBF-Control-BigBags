import { useOperation } from '../../context/OperationContext'

export function OperationsPage() {
  const { selectedShip, activeShift } = useOperation()

  return (
    <main className="app-shell">
      <header className="app-header">
        <div>
          <span className="eyebrow">IQBF Control</span>
          <h1>Operación en curso</h1>
        </div>
      </header>

      <section className="ship-selection" aria-labelledby="operations-title">
        <div className="section-heading">
          <div>
            <span className="eyebrow">Turno</span>
            <h2 id="operations-title">Resumen</h2>
          </div>
        </div>

        <div className="shift-summary" aria-live="polite">
          <div className="summary-row">
            <span className="summary-label">Nave</span>
            <strong>{selectedShip?.name ?? 'Sin nave'}</strong>
          </div>

          <div className="summary-row">
            <span className="summary-label">Turno</span>
            <strong>
              {activeShift
                ? `${activeShift.shipName} · ${activeShift.shiftDate}`
                : 'Sin turno activo'}
            </strong>
          </div>

          <div className="summary-row">
            <span className="summary-label">Horario</span>
            <strong>
              {activeShift
                ? activeShift.shiftType === 1
                  ? '06:00–18:00'
                  : '18:00–06:00'
                : 'Sin horario'}
            </strong>
          </div>
        </div>
      </section>
    </main>
  )
}
