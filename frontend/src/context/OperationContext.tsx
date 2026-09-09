import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import type { Ship } from '../types/ships'

interface OperationContextValue {
  selectedShip: Ship | null
  selectShip: (ship: Ship) => void
  clearOperation: () => void
}

const STORAGE_SHIP = 'selectedShip'

function getStoredShip(): Ship | null {
  const storedShip = localStorage.getItem(STORAGE_SHIP)
  if (!storedShip) return null

  try {
    const ship = JSON.parse(storedShip) as Partial<Ship>
    if (!ship.id || !ship.name) {
      localStorage.removeItem(STORAGE_SHIP)
      return null
    }

    return {
      id: String(ship.id),
      name: String(ship.name),
      status: ship.status ?? 'Active',
    }
  } catch {
    localStorage.removeItem(STORAGE_SHIP)
    return null
  }
}

const OperationContext = createContext<OperationContextValue | undefined>(undefined)

export function OperationProvider({ children }: { children: ReactNode }) {
  const [selectedShip, setSelectedShip] = useState<Ship | null>(getStoredShip)

  const selectShip = useCallback((ship: Ship) => {
    setSelectedShip(ship)
    localStorage.setItem(STORAGE_SHIP, JSON.stringify(ship))
  }, [])

  const clearOperation = useCallback(() => {
    setSelectedShip(null)
    localStorage.removeItem(STORAGE_SHIP)
  }, [])

  const value = useMemo(
    () => ({ selectedShip, selectShip, clearOperation }),
    [selectedShip, selectShip, clearOperation],
  )

  return <OperationContext.Provider value={value}>{children}</OperationContext.Provider>
}

export function useOperation(): OperationContextValue {
  const context = useContext(OperationContext)

  if (!context) {
    throw new Error('useOperation debe usarse dentro de OperationProvider')
  }

  return context
}