export interface OperationalMovementItem {
  blId: string
  blCode: string
  productName: string
  quantity: number
}

export interface OperationalMovementPhoto {
  id: string
  photoUrl: string
  fileName: string | null
  contentType: string | null
  fileSize: number | null
}

export interface OperationalMovement {
  id: string
  movementType: 'Reception' | 'Dispatch'
  transactionNumber: number
  createdAt: string
  createdBy: string | null
  reference: string
  comment: string | null
  items: OperationalMovementItem[]
  photos: OperationalMovementPhoto[]
}
