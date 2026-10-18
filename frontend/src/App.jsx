import { useCallback, useEffect, useState } from 'react'
import { api } from './api.js'
import Header from './components/Header.jsx'
import ProductList from './components/ProductList.jsx'
import ProductPanel from './components/ProductPanel.jsx'
import NewProductForm from './components/NewProductForm.jsx'
import Ledger from './components/Ledger.jsx'

export default function App() {
  const [info, setInfo] = useState(null)
  const [healthy, setHealthy] = useState(null)
  const [summary, setSummary] = useState(null)
  const [products, setProducts] = useState([])
  const [recent, setRecent] = useState(null)
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState('')

  const [search, setSearch] = useState('')
  const [criticalOnly, setCriticalOnly] = useState(false)
  const [showNew, setShowNew] = useState(false)

  const [selectedId, setSelectedId] = useState(null)
  const [selected, setSelected] = useState(null)
  const [ledger, setLedger] = useState(null)

  const loadList = useCallback(async () => {
    try {
      const [list, sum, rec] = await Promise.all([
        api.products({ search, criticalOnly }),
        api.summary(),
        api.recentMovements(12),
      ])
      setProducts(list)
      setSummary(sum)
      setRecent(rec)
      setLoadError('')
    } catch (e) {
      setLoadError(`Stok listesi alınamadı: ${e.message}`)
    } finally {
      setLoading(false)
    }
  }, [search, criticalOnly])

  const loadSelected = useCallback(async (id) => {
    if (!id) return
    const [p, l] = await Promise.all([api.product(id), api.ledger(id)])
    setSelected(p)
    setLedger(l)
  }, [])

  // First load: app info and health
  useEffect(() => {
    api.info().then(setInfo).catch(() => {})
    api.health().then(() => setHealthy(true)).catch(() => setHealthy(false))
  }, [])

  // Reload the list when the filters change (search is debounced a little)
  useEffect(() => {
    const t = setTimeout(loadList, search ? 200 : 0)
    return () => clearTimeout(t)
  }, [loadList, search])

  useEffect(() => {
    setLedger(null)
    loadSelected(selectedId).catch(() => setSelectedId(null))
  }, [selectedId, loadSelected])

  async function refreshAll() {
    await Promise.all([loadList(), loadSelected(selectedId)])
  }

  async function handleRecord(movement) {
    await api.recordMovement(selectedId, movement)
    await refreshAll()
  }

  async function handleSetCritical(level) {
    await api.setCriticalLevel(selectedId, level)
    await refreshAll()
  }

  async function handleCreate(body) {
    const created = await api.createProduct(body)
    setShowNew(false)
    setSearch('')
    setCriticalOnly(false)
    setSelectedId(created.id)
    await loadList()
  }

  function closePanel() {
    setSelectedId(null)
    setSelected(null)
  }

  return (
    <div className="shell">
      <Header
        info={info}
        summary={summary}
        healthy={healthy}
        onShowCritical={() => setCriticalOnly(true)}
      />

      <main className="layout">
        <section className="stock" aria-label="Stok listesi">
          <div className="toolbar">
            <label className="search">
              <span className="visually-hidden">Ürün ara</span>
              <input
                type="search"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Stok kodu ya da ürün adıyla ara"
              />
            </label>
            <label className="toggle">
              <input
                type="checkbox"
                checked={criticalOnly}
                onChange={(e) => setCriticalOnly(e.target.checked)}
              />
              <span>Sadece kritik</span>
            </label>
            <button
              type="button"
              className="secondary"
              onClick={() => setShowNew((v) => !v)}
              aria-expanded={showNew}
            >
              {showNew ? 'Formu kapat' : 'Yeni ürün'}
            </button>
          </div>

          {showNew && (
            <NewProductForm
              defaultCritical={info?.defaultCriticalLevel}
              onCreate={handleCreate}
              onCancel={() => setShowNew(false)}
            />
          )}

          {loadError && (
            <p className="form-error" role="alert">
              {loadError}
            </p>
          )}

          <ProductList
            products={products}
            selectedId={selectedId}
            onSelect={setSelectedId}
            loading={loading}
            filtered={Boolean(search.trim()) || criticalOnly}
          />
        </section>

        <aside className="side">
          {selected && selected.id === selectedId ? (
            <ProductPanel
              product={selected}
              ledger={ledger}
              onRecord={handleRecord}
              onSetCritical={handleSetCritical}
              onClose={closePanel}
            />
          ) : (
            <section className="panel panel--recent" aria-labelledby="recent-title">
              <h2 id="recent-title">Son hareketler</h2>
              <p className="panel__hint">Giriş ya da çıkış yapmak için soldan bir ürün seçin.</p>
              <Ledger
                movements={recent}
                showProduct
                onSelectProduct={setSelectedId}
                emptyText="Henüz hiç stok hareketi yok."
              />
            </section>
          )}
        </aside>
      </main>

      <footer className="footer">
        React, nginx ve ASP.NET Core ayrı container'larda çalışıyor.
        {info?.version && ` API sürümü ${info.version}, ortam ${info.environment}.`}
      </footer>
    </div>
  )
}
