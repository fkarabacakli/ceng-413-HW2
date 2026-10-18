/**
 * Stock shown like a tape measure: the fill is the current stock, the notch is
 * the critical level. The scale leaves room above the critical level so a
 * healthy item does not look full.
 */
export default function StockGauge({ stock, criticalLevel, size = 'row' }) {
  const max = Math.max(stock, criticalLevel * 3, 1)
  const fill = Math.min(100, (stock / max) * 100)
  const notch = Math.min(100, (criticalLevel / max) * 100)
  const state = stock === 0 ? 'empty' : stock <= criticalLevel ? 'critical' : 'ok'

  return (
    <div className={`gauge gauge--${size} gauge--${state}`} aria-hidden="true">
      <div className="gauge__fill" style={{ width: `${fill}%` }} />
      <div className="gauge__notch" style={{ left: `${notch}%` }} />
    </div>
  )
}
