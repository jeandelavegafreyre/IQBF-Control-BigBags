import apiClient from '../api/client'
import type { Ship } from '../types/ships'

export async function getActiveShips(): Promise<Ship[]> {
  const response = await apiClient.get<Ship[]>('/api/ships/active')
  return response.data
}