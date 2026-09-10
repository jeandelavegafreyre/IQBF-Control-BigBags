import apiClient from '../api/client'
import type { CreateDispatchRequest, Dispatch } from '../types/dispatches'

export async function createDispatch(request: CreateDispatchRequest): Promise<Dispatch> {
  const response = await apiClient.post<Dispatch>('/api/dispatches', request)
  return response.data
}
