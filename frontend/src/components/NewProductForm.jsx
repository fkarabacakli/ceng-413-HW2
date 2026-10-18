import { useState } from 'react'
import { UNITS, parseQty } from '../format.js'

const EMPTY = { sku: '', name: '', unit: 'Adet', criticalLevel: '', initialStock: '' }

export default function NewProductForm({ defaultCritical, onCreate, onCancel }) {
  const [form, setForm] = useState(EMPTY)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }))

  async function handleSubmit(event) {
    event.preventDefault()
    const criticalLevel = parseQty(form.criticalLevel)
    const initialStock = parseQty(form.initialStock)
    if (Number.isNaN(criticalLevel) || Number.isNaN(initialStock)) {
      setError('Kritik seviye ve açılış stoğu sayı olmalıdır.')
      return
    }

    setSaving(true)
    setError('')
    try {
      await onCreate({
        sku: form.sku,
        name: form.name,
        unit: form.unit,
        criticalLevel, // null → the API uses CRITICAL_STOCK_DEFAULT
        initialStock,
      })
      setForm(EMPTY)
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="new-product" onSubmit={handleSubmit} aria-label="Yeni ürün">
      <div className="new-product__grid">
        <label className="field">
          <span>Stok kodu</span>
          <input value={form.sku} onChange={set('sku')} placeholder="VD-4X40" maxLength={32} required autoFocus />
        </label>
        <label className="field field--wide">
          <span>Ürün adı</span>
          <input value={form.name} onChange={set('name')} placeholder="Sunta vidası 4x40 (200'lü)" maxLength={120} required />
        </label>
        <label className="field">
          <span>Birim</span>
          <select value={form.unit} onChange={set('unit')}>
            {UNITS.map((u) => (
              <option key={u.value} value={u.value}>
                {u.label}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span>Kritik seviye</span>
          <input
            inputMode="decimal"
            value={form.criticalLevel}
            onChange={set('criticalLevel')}
            placeholder={`Boş bırakılırsa ${defaultCritical ?? 5}`}
          />
        </label>
        <label className="field">
          <span>Açılış stoğu</span>
          <input inputMode="decimal" value={form.initialStock} onChange={set('initialStock')} placeholder="0" />
        </label>
      </div>

      {error && (
        <p className="form-error" role="alert">
          {error}
        </p>
      )}

      <div className="new-product__actions">
        <button type="submit" className="primary" disabled={saving}>
          {saving ? 'Ekleniyor…' : 'Ürünü ekle'}
        </button>
        <button type="button" className="link" onClick={onCancel}>
          Vazgeç
        </button>
      </div>
    </form>
  )
}
