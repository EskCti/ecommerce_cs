import type { PaymentStatus } from '../domain/finance.entity'

export function formatMoney(amount: unknown): string {
  const value = typeof amount === 'number' ? amount : Number(amount)
  if (!Number.isFinite(value)) return '—'
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export function formatDate(value: unknown): string {
  if (!value) return '—'
  const date = new Date(String(value))
  if (Number.isNaN(date.getTime())) return '—'
  return date.toLocaleDateString('pt-BR')
}

export function formatStatus(status: PaymentStatus | string | undefined): string {
  if (status === 'Open') return 'Em aberto'
  if (status === 'Settled') return 'Baixado'
  return status ? String(status) : '—'
}
