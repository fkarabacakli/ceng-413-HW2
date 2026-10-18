import { useState } from 'react'
import { parseQty } from '../format.js'

/** Stock in (Giriş) or stock out (Çıkış) for one product. */
export default function MovementForm({ product, onSubmit }) {
  const [type, setType] = useState('Out')
  const [quantity, setQuantity] = useState('')
  const [note, setNote] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const isIn = type === 'In'

  async function handleSubmit(event) {
    event.preventDefault()
    const value = parseQty(quantity)
    if (value === null || Number.isNaN(value)) {
      setError('Geçerli bir miktar girin.')
      return
    }

    setSaving(true)
    setError('')
    try {
      await onSubmit({ type, quantity: value, note: note.trim() || null })
      setQuantity('')
      setNote('')
    } catch (e) {
      setError(e.message) // e.g. "Stok yetersiz: ..." straight from the domain rule
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className={`movement movement--${isIn ? 'in' : 'out'}`} onSubmit={handleSubmit}>
      <div className="segmented" role="radiogroup" aria-label="Hareket tipi">
        <button type="button" role="radio" aria-checked={!isIn} onClick={() => setType('Out')}>
          Çıkış
        </button>
        <button type="button" role="radio" aria-checked={isIn} onClick={() => setType('In')}>
          Giriş
        </button>
      </div>

      <div className="movement__fields">
        <label className="field field--qty">
          <span>Miktar ({product.unitSymbol})</span>
          <input
            inputMode="decimal"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
            placeholder={product.unit === 'Adet' || product.unit === 'Paket' ? '1' : '0,5'}
            required
          />
        </label>
        <label className="field field--note">
          <span>Not</span>
          <input
            value={note}
            onChange={(e) => setNote(e.target.value)}
            maxLength={200}
            placeholder={isIn ? 'İrsaliye no, tedarikçi' : 'Perakende satış, usta adı'}
          />
        </label>
      </div>

      {error && (
        <p className="form-error" role="alert">
          {error}
        </p>
      )}

      <button type="submit" className="primary" disabled={saving}>
        {saving ? 'Kaydediliyor…' : isIn ? 'Girişi kaydet' : 'Çıkışı kaydet'}
      </button>
    </form>
  )
}
