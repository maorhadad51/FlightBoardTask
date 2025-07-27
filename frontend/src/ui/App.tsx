import React, { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createFlight, deleteFlight, getFlights, type Flight } from '../api'
import { AppBar, Box, Button, Container, Grid, MenuItem, Paper, Stack, TextField, Toolbar, Typography } from '@mui/material'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'

const schema = z.object({
  flightNumber: z.string().min(1, 'Required'),
  destination: z.string().min(1, 'Required'),
  gate: z.string().min(1, 'Required'),
  scheduledTime: z.string().min(1, 'Required')
})

export function App() {
  const queryClient = useQueryClient()
  const [filters, setFilters] = useState<{ destination?: string; status?: string }>({})

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['flights', filters],
    queryFn: () => getFlights(filters),
  })

  const deleteMut = useMutation({
    mutationFn: deleteFlight,
    onMutate: async (id: number) => {
      const key = ['flights', filters] as const
      await queryClient.cancelQueries({ queryKey: key })
      const prev = queryClient.getQueryData<Flight[]>(key)
      if (prev) {
        queryClient.setQueryData(key, prev.filter(f => f.id !== id))
      }
      return { prev }
    },
    onError: (_e, _id, ctx) => {
      const key = ['flights', filters] as const
      if (ctx?.prev) queryClient.setQueryData(key, ctx.prev)
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: ['flights'] })
  })

  const createMut = useMutation({
    mutationFn: createFlight,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['flights'] })
  })

  const { register, handleSubmit, reset, formState: { errors } } = useForm({
    resolver: zodResolver(schema)
  })

  const onSubmit = handleSubmit(async (val) => {
    await createMut.mutateAsync({
      flightNumber: val.flightNumber,
      destination: val.destination,
      gate: val.gate,
      scheduledTime: val.scheduledTime
    })
    reset()
  })

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6">FlightBoard</Typography>
        </Toolbar>
      </AppBar>

      <Container sx={{ mt: 2, mb: 4 }}>
        <Paper sx={{ p: 2, mb: 2 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={3}>
              <TextField label="Destination" fullWidth size="small"
                value={filters.destination ?? ''}
                onChange={e => setFilters(f => ({ ...f, destination: e.target.value }))} />
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField label="Status" fullWidth size="small" select
                value={filters.status ?? ''}
                onChange={e => setFilters(f => ({ ...f, status: e.target.value }))}>
                <MenuItem value="">All</MenuItem>
                {['Scheduled', 'Boarding', 'Departed', 'Landed'].map(s => <MenuItem key={s} value={s}>{s}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid item xs="auto">
              <Button variant="contained" onClick={() => refetch()}>Search</Button>
            </Grid>
            <Grid item xs="auto">
              <Button onClick={() => setFilters({})}>Clear Filters</Button>
            </Grid>
          </Grid>
        </Paper>

        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle1" sx={{ mb: 1 }}>Add Flight</Typography>
          <Box component="form" onSubmit={onSubmit}>
            <Stack direction="row" spacing={2} useFlexGap flexWrap="wrap">
              <TextField label="Flight Number" size="small" {...register('flightNumber')} error={!!errors.flightNumber} helperText={errors.flightNumber?.message as string} />
              <TextField label="Destination" size="small" {...register('destination')} error={!!errors.destination} helperText={errors.destination?.message as string} />
              <TextField label="Gate" size="small" {...register('gate')} error={!!errors.gate} helperText={errors.gate?.message as string} />
              <TextField label="Scheduled Time (UTC)" size="small" type="datetime-local" {...register('scheduledTime')} error={!!errors.scheduledTime} helperText={errors.scheduledTime?.message as string} />
              <Button type="submit" variant="contained">Add</Button>
            </Stack>
          </Box>
        </Paper>

        <Paper sx={{ p: 2 }}>
          <Typography variant="subtitle1" sx={{ mb: 1 }}>Flights</Typography>
          {isLoading ? 'Loading…' : (
            <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr>
                  <th style={{textAlign:'left', padding:'8px'}}>Flight #</th>
                  <th style={{textAlign:'left', padding:'8px'}}>Destination</th>
                  <th style={{textAlign:'left', padding:'8px'}}>Departure</th>
                  <th style={{textAlign:'left', padding:'8px'}}>Gate</th>
                  <th style={{textAlign:'left', padding:'8px'}}>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {(data ?? []).map(f => (
                  <tr key={f.id} style={{borderTop:'1px solid #eee'}}>
                    <td style={{padding:'8px'}}>{f.flightNumber}</td>
                    <td style={{padding:'8px'}}>{f.destination}</td>
                    <td style={{padding:'8px'}}>{new Date(f.scheduledTime).toLocaleString()}</td>
                    <td style={{padding:'8px'}}>{f.gate}</td>
                    <td style={{padding:'8px'}}>{f.status}</td>
                    <td style={{padding:'8px', textAlign:'right'}}>
                      <Button color="error" size="small" onClick={() => deleteMut.mutate(f.id)}>Delete</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Box>
          )}
        </Paper>
      </Container>
    </>
  )
}