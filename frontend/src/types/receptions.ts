export interface CreateReceptionRequest {
  shiftId: string
  terminalTruck: string
  comment?: string
  items: [{
    blId: string
    quantity: number
  }]
}

export interface ReceptionItem {
  blId: string
  blCode: string
  quantity: number
}

export interface Reception {
  id: string
  shiftId: string
  transactionNumber: number
  terminalTruck: string
  comment: string | null
  createdAt: string
  createdBy: string | null
  items: ReceptionItem[]
}
