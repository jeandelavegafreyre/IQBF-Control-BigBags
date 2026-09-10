import * as signalR from '@microsoft/signalr'

export function createOperationsConnection(token: string): signalR.HubConnection {
  const apiUrl = import.meta.env.VITE_API_URL as string
  const hubUrl = `${apiUrl.replace(/\/$/, '')}/hubs/operations`

  return new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build()
}
