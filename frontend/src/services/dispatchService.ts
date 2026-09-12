import apiClient from '../api/client'
import type { CreateDispatchRequest, Dispatch } from '../types/dispatches'

export async function createDispatch(request: CreateDispatchRequest): Promise<Dispatch> {
  const response = await apiClient.post<Dispatch>('/api/dispatches', request)
  return response.data
}

export async function getDispatchesByShift(shiftId: string): Promise<Dispatch[]> {
  const response = await apiClient.get<Dispatch[]>(`/api/dispatches/shift/${shiftId}`)
  return response.data
}

export async function updateDispatch(id: string, request: CreateDispatchRequest): Promise<Dispatch> {
  const response = await apiClient.put<Dispatch>(`/api/dispatches/${id}`, request)
  return response.data
}
