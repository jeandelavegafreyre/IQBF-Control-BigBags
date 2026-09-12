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
