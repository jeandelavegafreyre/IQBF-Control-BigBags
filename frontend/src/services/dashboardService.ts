import apiClient from '../api/client'
import type { ShipSummary, ShiftSummary } from '../types/dashboard'

export async function getShipSummary(shipId: string): Promise<ShipSummary> {
  const response = await apiClient.get<ShipSummary>(`/api/dashboard/ships/${shipId}/summary`)
  return response.data
}

export async function getShiftSummary(shiftId: string): Promise<ShiftSummary> {
  const response = await apiClient.get<ShiftSummary>(`/api/dashboard/shifts/${shiftId}/summary`)
  return response.data
}
