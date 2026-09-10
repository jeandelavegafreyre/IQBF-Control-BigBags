import apiClient from '../api/client'
import type { BL } from '../types/bls'

export async function getActiveBLsByShip(shipId: string): Promise<BL[]> {
  const response = await apiClient.get<BL[]>(`/api/bls/by-ship/${shipId}`)
  return response.data
}
