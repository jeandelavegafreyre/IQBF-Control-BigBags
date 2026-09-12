export interface DispatchItemRequest {
  blId: string
  quantity: number
}

export interface CreateDispatchRequest {
  shiftId: string
  plate: string
  comment?: string
  items: DispatchItemRequest[]
}

export interface DispatchItem {
  blId: string
  blCode: string
  quantity: number
}

export interface Dispatch {
  id: string
  shiftId: string
  transactionNumber: number
  plate: string
  comment: string | null
  createdAt: string
  createdBy: string | null
  updatedAt: string | null
  updatedBy: string | null
  items: DispatchItem[]
}
