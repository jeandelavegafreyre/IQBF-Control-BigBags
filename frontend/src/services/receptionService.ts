import apiClient from '../api/client'
import type { CreateReceptionRequest, Reception } from '../types/receptions'

export async function createReception(request: CreateReceptionRequest): Promise<Reception> {
  const response = await apiClient.post<Reception>('/api/receptions', request)
  return response.data
}

export async function getReceptionsByShift(shiftId: string): Promise<Reception[]> {
  const response = await apiClient.get<Reception[]>(`/api/receptions/shift/${shiftId}`)
  return response.data
}

export async function updateReception(id: string, request: CreateReceptionRequest): Promise<Reception> {
  const response = await apiClient.put<Reception>(`/api/receptions/${id}`, request)
  return response.data
}
