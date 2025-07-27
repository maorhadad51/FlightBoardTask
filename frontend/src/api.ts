import axios from 'axios'

export const API_URL = import.meta.env.VITE_API_URL ?? '' // relative: /api on nginx, http://localhost:5000 in dev if provided

export interface Flight {
  id: number
  flightNumber: string
  destination: string
  scheduledTime: string
  gate: string
  status: string
}

export async function getFlights(params: Record<string, string | number | boolean | undefined> = {}) {
  const qs = new URLSearchParams()
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== '') qs.set(k, String(v))
  })
  const res = await axios.get(`${API_URL}/api/flights${qs.toString() ? `?${qs}` : ''}`)
  return res.data as Flight[]
}

export async function createFlight(data: { flightNumber: string; destination: string; gate: string; scheduledTime: string }) {
  const res = await axios.post(`${API_URL}/api/flights`, data)
  return res.data as Flight
}

export async function deleteFlight(id: number) {
  await axios.delete(`${API_URL}/api/flights/${id}`)
  return id
}