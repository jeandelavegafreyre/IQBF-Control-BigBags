import apiClient from '../api/client'
import type { CreateReceptionRequest, Reception } from '../types/receptions'

export async function createReception(request: CreateReceptionRequest): Promise<Reception> {
  const response = await apiClient.post<Reception>('/api/receptions', request)
  return response.data
}
