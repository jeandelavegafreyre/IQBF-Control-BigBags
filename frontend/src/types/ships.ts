export type ShipStatus = 'Active' | 'Inactive' | number

export interface Ship {
  id: string
  name: string
  status: ShipStatus
}