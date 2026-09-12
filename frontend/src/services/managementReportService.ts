import apiClient from '../api/client'
import type { ManagementReport } from '../types/managementReports'

export async function getManagementReport(year: number, month: number): Promise<ManagementReport> {
  const response = await apiClient.get<ManagementReport>('/api/reports/management', {
    params: { year, month },
  })
  return response.data
}
