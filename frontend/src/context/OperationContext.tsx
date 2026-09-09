import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import type { Ship } from '../types/ships'
import type { Shift } from '../types/shifts'

interface OperationContextValue {
  selectedShip: Ship | null
  activeShift: Shift | null
  selectShip: (ship: Ship) => void
  setActiveShift: (shift: Shift) => void
  clearActiveShift: () => void
  clearOperation: () => void
}

const STORAGE_SHIP = 'selectedShip'
const STORAGE_SHIFT = 'activeShift'

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

function getStoredShift(): Shift | null {
  const storedShift = localStorage.getItem(STORAGE_SHIFT)
  if (!storedShift) return null

  try {
    const shift = JSON.parse(storedShift) as Partial<Shift>
    if (
      !shift.id ||
      !shift.shiftDate ||
      !shift.shipId ||
      !shift.shipName ||
      (shift.shiftType !== 1 && shift.shiftType !== 2) ||
      (shift.status !== 1 && shift.status !== 2)
    ) {
      localStorage.removeItem(STORAGE_SHIFT)
      return null
    }

    return {
      id: String(shift.id),
      shiftDate: String(shift.shiftDate),
      shiftType: shift.shiftType,
      status: shift.status,
      startedAt: String(shift.startedAt ?? ''),
      endedAt: shift.endedAt ? String(shift.endedAt) : null,
      shipId: String(shift.shipId),
      shipName: String(shift.shipName),
    }
  } catch {
    localStorage.removeItem(STORAGE_SHIFT)
    return null
  }
}

const OperationContext = createContext<OperationContextValue | undefined>(undefined)

export function OperationProvider({ children }: { children: ReactNode }) {
  const [selectedShip, setSelectedShip] = useState<Ship | null>(getStoredShip)
  const [activeShift, setActiveShiftState] = useState<Shift | null>(getStoredShift)

  const selectShip = useCallback((ship: Ship) => {
    setSelectedShip(ship)
    localStorage.setItem(STORAGE_SHIP, JSON.stringify(ship))

    setActiveShiftState((currentShift) => {
      if (!currentShift || currentShift.shipId === ship.id) {
        return currentShift
      }

      localStorage.removeItem(STORAGE_SHIFT)
      return null
    })
  }, [])

  const setActiveShift = useCallback((shift: Shift) => {
    setActiveShiftState(shift)
    localStorage.setItem(STORAGE_SHIFT, JSON.stringify(shift))
  }, [])

  const clearActiveShift = useCallback(() => {
    setActiveShiftState(null)
    localStorage.removeItem(STORAGE_SHIFT)
  }, [])

  const clearOperation = useCallback(() => {
    setSelectedShip(null)
    setActiveShiftState(null)
    localStorage.removeItem(STORAGE_SHIP)
    localStorage.removeItem(STORAGE_SHIFT)
  }, [])

  const value = useMemo(
    () => ({
      selectedShip,
      activeShift,
      selectShip,
      setActiveShift,
      clearActiveShift,
      clearOperation,
    }),
    [selectedShip, activeShift, selectShip, setActiveShift, clearActiveShift, clearOperation],
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