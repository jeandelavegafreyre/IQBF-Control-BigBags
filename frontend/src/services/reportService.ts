import apiClient from '../api/client'
import type { OperationalMovement } from '../types/reports'

export async function getShiftMovements(shiftId: string): Promise<OperationalMovement[]> {
  const response = await apiClient.get<OperationalMovement[]>(`/api/reports/shifts/${shiftId}/movements`)
  return response.data
}
