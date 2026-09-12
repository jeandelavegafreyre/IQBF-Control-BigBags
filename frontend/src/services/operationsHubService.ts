import * as signalR from '@microsoft/signalr'

function getActiveShiftId(): string {
  try {
    const raw = localStorage.getItem('activeShift')
    if (!raw) return ''
    const shift = JSON.parse(raw) as { id?: string }
    return typeof shift.id === 'string' ? shift.id : ''
  } catch {
    return ''
  }
}

function invalidateSession(message?: string) {
  localStorage.removeItem('token')
  localStorage.removeItem('authUser')
  sessionStorage.setItem(
    'sessionMessage',
    message || 'Tu sesión ya no es válida. Inicia sesión nuevamente.',
  )

  if (window.location.pathname !== '/login') {
    window.location.replace('/login')
  }
}

export function createOperationsConnection(token: string): signalR.HubConnection {
  const apiUrl = import.meta.env.VITE_API_URL as string
  const shiftId = getActiveShiftId()
  const baseHubUrl = `${apiUrl.replace(/\/$/, '')}/hubs/operations`
  const hubUrl = shiftId ? `${baseHubUrl}?shiftId=${encodeURIComponent(shiftId)}` : baseHubUrl

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build()

  connection.on('SessionRevoked', (message?: string) => {
    invalidateSession(message)
  })

  return connection
}
