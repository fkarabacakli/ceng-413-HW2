import StockGauge from './StockGauge.jsx'
import MovementForm from './MovementForm.jsx'
import CriticalLevelEditor from './CriticalLevelEditor.jsx'
import Ledger from './Ledger.jsx'
import { formatQty } from '../format.js'

export default function ProductPanel({ product, ledger, onRecord, onSetCritical, onClose }) {
  const state = product.stock === 0 ? 'empty' : product.isCritical ? 'critical' : 'ok'

  return (
    <section className={`panel panel--${state}`} aria-labelledby="panel-title">
      <div className="panel__top">
        <div>
          <p className="panel__sku">{product.sku}</p>
          <h2 id="panel-title">{product.name}</h2>
        </div>
        <button type="button" className="close" onClick={onClose} aria-label="Ürün detayını kapat">
          ×
        </button>
      </div>

      <p className="panel__stock">
        <span className="panel__number">{formatQty(product.stock, product.unitSymbol)}</span>
        <span className="panel__state">
          {state === 'empty' ? 'Rafta kalmadı' : state === 'critical' ? 'Sipariş zamanı' : 'Rafta yeterli'}
        </span>
      </p>
      <StockGauge stock={product.stock} criticalLevel={product.criticalLevel} size="large" />

      <CriticalLevelEditor key={product.id} product={product} onSave={onSetCritical} />

      <MovementForm key={`${product.id}-form`} product={product} onSubmit={onRecord} />

      <h3>Stok defteri</h3>
      <Ledger movements={ledger} />
    </section>
  )
}
