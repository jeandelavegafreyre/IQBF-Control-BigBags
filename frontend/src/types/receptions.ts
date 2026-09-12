export interface CreateReceptionItemRequest {
  blId: string
  quantity: number
}

export interface CreateReceptionRequest {
  shiftId: string
  terminalTruck: string
  comment?: string
  items: CreateReceptionItemRequest[]
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
  updatedAt: string | null
  updatedBy: string | null
  items: ReceptionItem[]
}
