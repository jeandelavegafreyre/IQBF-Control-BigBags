export interface ManagementShipReport {
  shipId: string
  shipName: string
  firstReceptionAt: string | null
  lastDispatchAt: string
  declaredQuantity: number
  receivedQuantity: number
  dispatchedQuantity: number
  availableQuantity: number
  receptionTransactions: number
  dispatchTransactions: number
  calendarDurationHours: number
  blCount: number
  products: string[]
}

export interface ManagementMonthlyTrend {
  month: number
  completedShips: number
  dispatchedQuantity: number
}

export interface ManagementProductReport {
  productName: string
  declaredQuantity: number
  receivedQuantity: number
  dispatchedQuantity: number
  shipCount: number
}

export interface ManagementReport {
  year: number
  month: number
  completedShips: number
  totalReceived: number
  totalDispatched: number
  totalDeclared: number
  ships: ManagementShipReport[]
  monthlyTrend: ManagementMonthlyTrend[]
  products: ManagementProductReport[]
}

export interface ManagementInProcessShip {
  shipId: string
  shipName: string
  firstReceptionAt: string
  lastDispatchAt: string | null
  lastMovementAt: string
  declaredQuantity: number
  receivedQuantity: number
  dispatchedQuantity: number
  availableQuantity: number
  dispatchProgress: number
  calendarDurationHours: number
  blCount: number
  receptionTransactions: number
  dispatchTransactions: number
  products: string[]
}

export interface ManagementInProcessReport {
  shipsInProcess: number
  totalReceived: number
  totalDispatched: number
  totalAvailable: number
  ships: ManagementInProcessShip[]
}
