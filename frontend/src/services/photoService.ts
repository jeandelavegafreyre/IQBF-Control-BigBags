import apiClient from '../api/client'
import type { PhotoEvidence } from '../types/photos'

function buildPhotoFormData(file: File): FormData {
  const data = new FormData()
  data.append('file', file)
  return data
}

export async function uploadReceptionPhoto(receptionId: string, file: File): Promise<PhotoEvidence> {
  const response = await apiClient.post<PhotoEvidence>(
    `/api/receptions/${receptionId}/photos`,
    buildPhotoFormData(file),
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    },
  )
  return response.data
}

export async function uploadDispatchPhoto(dispatchId: string, file: File): Promise<PhotoEvidence> {
  const response = await apiClient.post<PhotoEvidence>(
    `/api/dispatches/${dispatchId}/photos`,
    buildPhotoFormData(file),
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    },
  )
  return response.data
}
