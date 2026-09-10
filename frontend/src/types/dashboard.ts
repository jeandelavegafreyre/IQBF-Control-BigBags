import type { ShiftStatus, ShiftType } from './shifts'

export interface ShiftBLBalance {
  blId: string
  blCode: string
  productName: string
  receivedQuantity: number
  dispatchedQuantity: number
  netQuantity: number
}

export interface ShiftSummary {
  shiftId: string
  shiftDate: string
  shiftType: ShiftType
  status: ShiftStatus
  startedAt: string
  endedAt: string | null
  shipId: string
  shipName: string
  receivedQuantity: number
  dispatchedQuantity: number
  netQuantity: number
  bls: ShiftBLBalance[]
}

export interface ShipBLBalance {
  id: string
  code: string
  productName: string
  totalQuantity: number
  receivedQuantity: number
  dispatchedQuantity: number
  availableQuantity: number
  pendingReception: number
  receptionProgress: number
  dispatchProgress: number
}

export interface ShipSummary {
  shipId: string
  shipName: string
  totalQuantity: number
  receivedQuantity: number
  dispatchedQuantity: number
  availableQuantity: number
  pendingReception: number
  receptionProgress: number
  dispatchProgress: number
  bLs: ShipBLBalance[]
}
