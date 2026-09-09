import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { loginUser } from '../services/authService'
import type { AuthResponse, AuthUser, LoginRequest } from '../types/auth'

interface AuthContextValue {
  user: AuthUser | null
  token: string | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (payload: LoginRequest) => Promise<void>
  logout: () => void
}

const STORAGE_TOKEN = 'token'
const STORAGE_USER = 'authUser'

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function parseStoredUser(rawValue: string | null): AuthUser | null {
  if (!rawValue) return null

  try {
    const parsed = JSON.parse(rawValue) as Partial<AuthUser>
    if (!parsed.uid || !parsed.fullName) return null

    return {
      userId: String(parsed.userId ?? ''),
      uid: String(parsed.uid),
      fullName: String(parsed.fullName),
      role: String(parsed.role ?? 'Unknown'),
      expiresAtUtc: String(parsed.expiresAtUtc ?? ''),
    }
  } catch {
    return null
  }
}

function getStoredSession(): { token: string | null; user: AuthUser | null } {
  const token = localStorage.getItem(STORAGE_TOKEN)
  const storedUser = parseStoredUser(localStorage.getItem(STORAGE_USER))

  if (!token || !storedUser) {
    return { token: null, user: null }
  }

  const expiresAt = storedUser.expiresAtUtc
  if (expiresAt) {
    const expiresAtMs = new Date(expiresAt).getTime()
    if (!Number.isNaN(expiresAtMs) && expiresAtMs <= Date.now()) {
      localStorage.removeItem(STORAGE_TOKEN)
      localStorage.removeItem(STORAGE_USER)
      return { token: null, user: null }
    }
  }

  return { token, user: storedUser }
}

function toAuthUser(response: AuthResponse): AuthUser {
  return {
    userId: response.userId,
    uid: response.uid,
    fullName: response.fullName,
    role: response.role,
    expiresAtUtc: response.expiresAtUtc,
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [token, setToken] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const { token: storedToken, user: storedUser } = getStoredSession()
    setToken(storedToken)
    setUser(storedUser)
    setIsLoading(false)
  }, [])

  const login = useCallback(async (payload: LoginRequest) => {
    const response = await loginUser(payload)
    const authUser = toAuthUser(response)

    setToken(response.token)
    setUser(authUser)

    localStorage.setItem(STORAGE_TOKEN, response.token)
    localStorage.setItem(STORAGE_USER, JSON.stringify(authUser))
  }, [])

  const logout = useCallback(() => {
    setToken(null)
    setUser(null)
    localStorage.removeItem(STORAGE_TOKEN)
    localStorage.removeItem(STORAGE_USER)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isAuthenticated: Boolean(token && user),
      isLoading,
      login,
      logout,
    }),
    [login, logout, token, user, isLoading],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth debe usarse dentro de AuthProvider')
  }

  return context
}
