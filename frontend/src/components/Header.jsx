import { today } from '../format.js'

export default function Header({ info, summary, healthy, onShowCritical }) {
  return (
    <header className="header">
      <div className="header__brand">
        <h1>{info?.shopName ?? 'Nalbur Stok'}</h1>
        <p className="header__date">{today()}</p>
      </div>

      <dl className="stats">
        <div className="stat">
          <dt>Ürün</dt>
          <dd>{summary?.productCount ?? '–'}</dd>
        </div>
        <button
          type="button"
          className={`stat stat--button ${summary?.criticalCount ? 'stat--warn' : ''}`}
          onClick={onShowCritical}
          disabled={!summary?.criticalCount}
          title="Kritik seviyedeki ürünleri listele"
        >
          <dt>Kritik</dt>
          <dd>{summary?.criticalCount ?? '–'}</dd>
        </button>
        <div className={`stat ${summary?.outOfStockCount ? 'stat--danger' : ''}`}>
          <dt>Tükenen</dt>
          <dd>{summary?.outOfStockCount ?? '–'}</dd>
        </div>
        <div className="stat">
          <dt>Son 24 saatte hareket</dt>
          <dd>{summary?.movementsLast24h ?? '–'}</dd>
        </div>
      </dl>

      <p className={`api-state api-state--${healthy === null ? 'unknown' : healthy ? 'ok' : 'down'}`}>
        {healthy === null ? 'API kontrol ediliyor' : healthy ? 'API çalışıyor' : 'API yanıt vermiyor'}
      </p>
    </header>
  )
}
