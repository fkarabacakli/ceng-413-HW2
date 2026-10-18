const qty = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 })
const relative = new Intl.RelativeTimeFormat('tr', { numeric: 'auto' })
const dateTime = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit',
})
const longDate = new Intl.DateTimeFormat('tr-TR', { weekday: 'long', day: 'numeric', month: 'long' })

export const formatQty = (value, unit) => `${qty.format(value)} ${unit}`

export function timeAgo(iso) {
  if (!iso) return 'hareket yok'
  const seconds = (new Date(iso).getTime() - Date.now()) / 1000
  const steps = [
    ['day', 86400],
    ['hour', 3600],
    ['minute', 60],
  ]
  for (const [unit, size] of steps) {
    if (Math.abs(seconds) >= size) return relative.format(Math.round(seconds / size), unit)
  }
  return 'az önce'
}

export const formatDateTime = (iso) => dateTime.format(new Date(iso))
export const today = () => longDate.format(new Date())

export const UNITS = [
  { value: 'Adet', label: 'Adet', whole: true },
  { value: 'Paket', label: 'Paket', whole: true },
  { value: 'Kg', label: 'Kilogram', whole: false },
  { value: 'Metre', label: 'Metre', whole: false },
  { value: 'Litre', label: 'Litre', whole: false },
]

/** Accepts both "2,5" (Turkish keyboard) and "2.5". */
export function parseQty(text) {
  const normalized = String(text).trim().replace(',', '.')
  if (normalized === '') return null
  const n = Number(normalized)
  return Number.isFinite(n) ? n : NaN
}
