import apiClient from '../api/client'
import type { ManagementInProcessReport, ManagementReport } from '../types/managementReports'

export async function getManagementReport(year: number, month: number): Promise<ManagementReport> {
  const response = await apiClient.get<ManagementReport>('/api/reports/management', {
    params: { year, month },
  })
  return response.data
}

export async function getManagementInProcess(): Promise<ManagementInProcessReport> {
  const response = await apiClient.get<ManagementInProcessReport>('/api/reports/management/active-ships')
  return response.data
}
