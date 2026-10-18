// All calls use relative URLs (/api/...). The browser sends them to the origin
// that served the page; nginx (in Docker) or Vite (in dev) forwards them to the API.
// So the frontend image never needs to know the API address at build time.

async function request(path, options = {}) {
  const response = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })

  if (!response.ok) {
    let message = `İstek başarısız oldu (${response.status}).`
    try {
      const problem = await response.json()
      message = problem.detail || problem.title || message
    } catch {
      // no JSON body
    }
    throw new Error(message)
  }

  return response.status === 204 ? null : response.json()
}

const json = (body) => JSON.stringify(body)

export const api = {
  health: () => request('/health'),
  info: () => request('/api/info'),
  summary: () => request('/api/summary'),

  products: ({ search = '', criticalOnly = false } = {}) => {
    const params = new URLSearchParams()
    if (search.trim()) params.set('search', search.trim())
    if (criticalOnly) params.set('critical', 'true')
    const qs = params.toString()
    return request(`/api/products${qs ? `?${qs}` : ''}`)
  },
  product: (id) => request(`/api/products/${id}`),
  createProduct: (body) => request('/api/products', { method: 'POST', body: json(body) }),
  setCriticalLevel: (id, criticalLevel) =>
    request(`/api/products/${id}/critical-level`, { method: 'PUT', body: json({ criticalLevel }) }),

  ledger: (id) => request(`/api/products/${id}/movements`),
  recordMovement: (id, body) =>
    request(`/api/products/${id}/movements`, { method: 'POST', body: json(body) }),
  recentMovements: (take = 12) => request(`/api/movements?take=${take}`),
}
