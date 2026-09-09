export type ShiftType = 1 | 2
export type ShiftStatus = 1 | 2

export interface Shift {
  id: string
  shiftDate: string
  shiftType: ShiftType
  status: ShiftStatus
  startedAt: string
  endedAt: string | null
  shipId: string
  shipName: string
}

export interface StartShiftRequest {
  shipId: string
  shiftDate: string
  shiftType: ShiftType
}