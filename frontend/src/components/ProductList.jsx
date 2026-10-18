import StockGauge from './StockGauge.jsx'
import { formatQty, timeAgo } from '../format.js'

export default function ProductList({ products, selectedId, onSelect, loading, filtered }) {
  if (loading) {
    return <p className="list-message">Ürünler yükleniyor…</p>
  }

  if (products.length === 0) {
    return (
      <p className="list-message">
        {filtered
          ? 'Bu aramaya uyan ürün yok. Aramayı temizleyin ya da filtreyi kapatın.'
          : 'Henüz ürün yok. Sağ üstteki "Yeni ürün" ile ilk ürünü ekleyin.'}
      </p>
    )
  }

  return (
    <ul className="products" aria-label="Ürünler">
      {products.map((p) => {
        const state = p.stock === 0 ? 'empty' : p.isCritical ? 'critical' : 'ok'
        return (
          <li key={p.id}>
            <button
              type="button"
              className={`bin bin--${state} ${p.id === selectedId ? 'bin--selected' : ''}`}
              onClick={() => onSelect(p.id)}
              aria-pressed={p.id === selectedId}
            >
              <span className="bin__sku">{p.sku}</span>
              <span className="bin__name">{p.name}</span>
              <span className="bin__stock">
                <strong>{formatQty(p.stock, p.unitSymbol)}</strong>
                {state === 'empty' && <em className="tag tag--empty">Tükendi</em>}
                {state === 'critical' && <em className="tag tag--critical">Kritik</em>}
              </span>
              <StockGauge stock={p.stock} criticalLevel={p.criticalLevel} />
              <span className="bin__meta">
                Kritik seviye {formatQty(p.criticalLevel, p.unitSymbol)}, son hareket {timeAgo(p.lastMovementAtUtc)}
              </span>
            </button>
          </li>
        )
      })}
    </ul>
  )
}
