import apiClient from '../api/client'
import type { Ship } from '../types/ships'
import type { BL } from '../types/bls'
import type { Product, UserSummary } from '../types/admin'

export async function createShip(name: string): Promise<Ship> {
  return (await apiClient.post<Ship>('/api/ships', { name })).data
}
export async function getProducts(): Promise<Product[]> {
  return (await apiClient.get<Product[]>('/api/products')).data
}
export async function createProduct(name: string): Promise<Product> {
  return (await apiClient.post<Product>('/api/products', { name })).data
}
export async function createBL(code: string, totalQuantity: number, shipId: string, productId: string): Promise<BL> {
  return (await apiClient.post<BL>('/api/bls', { code, totalQuantity, shipId, productId })).data
}
export async function getUsers(): Promise<UserSummary[]> {
  return (await apiClient.get<UserSummary[]>('/api/users')).data
}
export async function createUser(uid: string, firstName: string, lastName: string, password: string, role: number): Promise<void> {
  await apiClient.post('/api/auth/register', { uid, firstName, lastName, password, role })
}
export async function updateUserRole(userId: string, role: number): Promise<void> {
  await apiClient.put(`/api/users/${userId}/role`, { role })
}
