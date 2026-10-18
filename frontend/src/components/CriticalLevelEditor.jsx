import { useState } from 'react'
import { formatQty, parseQty } from '../format.js'

export default function CriticalLevelEditor({ product, onSave }) {
  const [editing, setEditing] = useState(false)
  const [value, setValue] = useState('')
  const [error, setError] = useState('')

  function start() {
    setValue(String(product.criticalLevel).replace('.', ','))
    setError('')
    setEditing(true)
  }

  async function save(event) {
    event.preventDefault()
    const parsed = parseQty(value)
    if (parsed === null || Number.isNaN(parsed)) {
      setError('Geçerli bir sayı girin.')
      return
    }
    try {
      await onSave(parsed)
      setEditing(false)
    } catch (e) {
      setError(e.message)
    }
  }

  if (!editing) {
    return (
      <p className="critical-line">
        Kritik seviye <strong>{formatQty(product.criticalLevel, product.unitSymbol)}</strong>
        <button type="button" className="link" onClick={start}>
          Değiştir
        </button>
      </p>
    )
  }

  return (
    <form className="critical-edit" onSubmit={save}>
      <label className="field">
        <span>Yeni kritik seviye ({product.unitSymbol})</span>
        <input inputMode="decimal" value={value} onChange={(e) => setValue(e.target.value)} autoFocus />
      </label>
      <button type="submit" className="secondary">
        Kaydet
      </button>
      <button type="button" className="link" onClick={() => setEditing(false)}>
        Vazgeç
      </button>
      {error && (
        <p className="form-error" role="alert">
          {error}
        </p>
      )}
    </form>
  )
}
