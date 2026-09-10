import apiClient from '../api/client'
import type { Shift, StartShiftRequest } from '../types/shifts'

export async function getOpenShift(shipId: string): Promise<Shift | null> {
  const response = await apiClient.get<Shift | null>('/api/shifts/open', {
    params: { shipId },
    validateStatus: (status) => status === 200 || status === 204,
  })

  return response.status === 204 ? null : response.data
}

export async function startShift(request: StartShiftRequest): Promise<Shift> {
  const response = await apiClient.post<Shift>('/api/shifts/start', request)
  return response.data
}

export async function closeShift(shiftId: string): Promise<void> {
  await apiClient.post(`/api/shifts/${shiftId}/close`)
}
