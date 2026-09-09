import apiClient from '../api/client'
import type { AuthResponse, LoginRequest } from '../types/auth'

function normalizeRole(value: unknown): string {
  if (typeof value === 'number') {
    if (value === 1) return 'Administrator'
    if (value === 2) return 'Yard'
    if (value === 3) return 'User'
    return 'Unknown'
  }

  const text = String(value ?? '').trim()
  return text || 'Unknown'
}

function normalizeAuthResponse(data: AuthResponse): AuthResponse {
  if (!data.token || !data.uid) {
    throw new Error('La respuesta del servidor no incluye la sesión autenticada.')
  }

  return data
}

export async function loginUser(payload: LoginRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/login', {
    uid: payload.uid.trim(),
    password: payload.password,
  })

  return normalizeAuthResponse(response.data)
}
