export interface LoginRequest {
  uid: string
  password: string
}

export interface AuthUser {
  userId: string
  uid: string
  fullName: string
  role: string
  expiresAtUtc: string
}

export interface AuthResponse {
  token: string
  expiresAtUtc: string
  userId: string
  uid: string
  fullName: string
  role: string
}
