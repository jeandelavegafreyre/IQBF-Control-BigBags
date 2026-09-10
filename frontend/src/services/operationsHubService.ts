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

export function createOperationsConnection(token: string): signalR.HubConnection {
  const apiUrl = import.meta.env.VITE_API_URL as string
  const shiftId = getActiveShiftId()
  const baseHubUrl = `${apiUrl.replace(/\/$/, '')}/hubs/operations`
  const hubUrl = shiftId ? `${baseHubUrl}?shiftId=${encodeURIComponent(shiftId)}` : baseHubUrl

  return new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build()
}
