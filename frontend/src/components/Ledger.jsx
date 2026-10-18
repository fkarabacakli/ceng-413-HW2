import { formatDateTime, formatQty } from '../format.js'

/**
 * The stock ledger (stok defteri). Lines are never edited: a mistake is fixed
 * with a new opposite movement, so the balance column always adds up.
 */
export default function Ledger({
  movements,
  showProduct = false,
  onSelectProduct,
  emptyText = 'Bu ürün için henüz hareket yok.',
}) {
  if (!movements) {
    return <p className="list-message">Hareketler yükleniyor…</p>
  }

  if (movements.length === 0) {
    return <p className="list-message">{emptyText}</p>
  }

  return (
    <div className="ledger-wrap">
    <table className="ledger">
      <thead>
        <tr>
          <th scope="col">Tarih</th>
          {showProduct ? (
            <>
              <th scope="col">Ürün</th>
              <th scope="col" className="num">Hareket</th>
            </>
          ) : (
            <>
              <th scope="col" className="num">Giriş</th>
              <th scope="col" className="num">Çıkış</th>
            </>
          )}
          <th scope="col" className="num">Bakiye</th>
        </tr>
      </thead>
      <tbody>
        {movements.map((m) => (
          <tr key={m.id}>
            <td>
              <span className="ledger__date">{formatDateTime(m.createdAtUtc)}</span>
              {m.note && <span className="ledger__note">{m.note}</span>}
            </td>
            {showProduct ? (
              <>
                <td>
                  <button type="button" className="link" onClick={() => onSelectProduct?.(m.productId)}>
                    {m.productName}
                  </button>
                </td>
                {/* One signed column keeps the overview narrow: +24 in, −6 out */}
                <td className={`num ${m.type === 'In' ? 'signed-in' : ''}`}>
                  {m.type === 'In' ? '+' : '−'}
                  {formatQty(m.quantity, m.unitSymbol)}
                </td>
              </>
            ) : (
              <>
                <td className="num in">{m.type === 'In' ? formatQty(m.quantity, m.unitSymbol) : ''}</td>
                <td className="num out">{m.type === 'Out' ? formatQty(m.quantity, m.unitSymbol) : ''}</td>
              </>
            )}
            <td className="num balance">{formatQty(m.balanceAfter, m.unitSymbol)}</td>
          </tr>
        ))}
      </tbody>
    </table>
    </div>
  )
}
