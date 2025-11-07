import React, { useEffect, useState } from 'react'
import './styles.css'

function Register({ apiBase, onToken }) {
  const [form, setForm] = useState({ email: '', fullName: '', password: '', passwordConfirmation: '' })
  const [result, setResult] = useState(null)
  const onChange = e => setForm({ ...form, [e.target.name]: e.target.value })
  const submit = async e => {
    e.preventDefault()
    setResult('Submitting...')
    try {
  const res = await fetch(`${apiBase}/api/account/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(form)
      })
      if (res.status === 201) {
        // Auto sign-in after successful registration to obtain token
  const signin = await fetch(`${apiBase}/api/account/signin`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ email: form.email, password: form.password })
        })
        const body = await signin.json().catch(() => ({}))
        if (signin.ok && body.token) {
          onToken(body.token)
          setResult('Registered and signed in')
          return
        }
      }
      setResult(`${res.status} ${res.statusText}`)
    } catch (err) {
      setResult(`Error: ${err}`)
    }
  }
  return (
    <div className="card">
      <h2>Register</h2>
      <form onSubmit={submit}>
        <div className="field">
          <label htmlFor="reg-email">Email</label>
          <input id="reg-email" name="email" placeholder="you@example.com" value={form.email} onChange={onChange} />
        </div>
        <div className="field">
          <label htmlFor="reg-name">Full name</label>
          <input id="reg-name" name="fullName" placeholder="Jane Doe" value={form.fullName} onChange={onChange} />
        </div>
        <div className="field">
          <label htmlFor="reg-pass">Password</label>
          <input id="reg-pass" name="password" type="password" placeholder="••••••••" value={form.password} onChange={onChange} />
        </div>
        <div className="field">
          <label htmlFor="reg-pass2">Confirm password</label>
          <input id="reg-pass2" name="passwordConfirmation" type="password" placeholder="••••••••" value={form.passwordConfirmation} onChange={onChange} />
        </div>
  <button className="btn" type="submit" title="POST /api/account/register">Register</button>
      </form>
      {result && <p className="status">{result}</p>}
    </div>
  )
}

function SignIn({ apiBase, onToken }) {
  const [form, setForm] = useState({ email: '', password: '' })
  const [result, setResult] = useState(null)
  const onChange = e => setForm({ ...form, [e.target.name]: e.target.value })
  const submit = async e => {
    e.preventDefault()
    setResult('Submitting...')
    try {
  const res = await fetch(`${apiBase}/api/account/signin`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(form)
      })
      const body = await res.json().catch(() => ({}))
      if (res.ok && body.token) {
        onToken(body.token)
        setResult('Signed in')
      } else {
        setResult(`${res.status} ${res.statusText}`)
      }
    } catch (err) {
      setResult(`Error: ${err}`)
    }
  }
  return (
    <div className="card">
      <h2>Sign In</h2>
      <form onSubmit={submit}>
        <div className="field">
          <label htmlFor="si-email">Email</label>
          <input id="si-email" name="email" placeholder="you@example.com" value={form.email} onChange={onChange} />
        </div>
        <div className="field">
          <label htmlFor="si-pass">Password</label>
          <input id="si-pass" name="password" type="password" placeholder="••••••••" value={form.password} onChange={onChange} />
        </div>
  <button className="btn" type="submit" title="POST /api/account/signin">Sign In</button>
      </form>
      {result && <p className="status">{result}</p>}
    </div>
  )
}

function SignOut({ apiBase, token, onSignedOut }) {
  const [result, setResult] = useState(null)
  const signout = async () => {
    try {
  const res = await fetch(`${apiBase}/api/account/signout`, {
        method: 'GET',
        headers: { 'Authorization': `Bearer ${token}` }
      })
      setResult(`${res.status} ${res.statusText}`)
      if (res.status === 204) onSignedOut()
    } catch (err) {
      setResult(`Error: ${err}`)
    }
  }
  return (
    <div className="card" style={{ maxWidth: 380 }}>
      <h2>Sign Out</h2>
  <button className="btn" onClick={signout} disabled={!token} title="GET /api/account/signout">Sign Out</button>
      {result && <p className="status">{result}</p>}
    </div>
  )
}

export default function App() {
  const [token, setToken] = useState('')
  const [cryptos, setCryptos] = useState([])
  const [cryptoPage, setCryptoPage] = useState(() => {
    const v = parseInt(localStorage.getItem('cryptoPage') || '1', 10)
    return Number.isFinite(v) && v > 0 ? v : 1
  })
  const [exchanges, setExchanges] = useState([])
  const [exchangesResult, setExchangesResult] = useState('')
  const [exchangesLoading, setExchangesLoading] = useState(false)
  const [exchangesPage, setExchangesPage] = useState(1)
  const [config, setConfig] = useState(null)
  const [cartItems, setCartItems] = useState([])
  const [activeTab, setActiveTab] = useState('cryptos') // 'cryptos' | 'exchanges'
  const [profile, setProfile] = useState({ email: '', fullName: '' })
  const [addrList, setAddrList] = useState([])
  const [addrForm, setAddrForm] = useState({ streetName: '', houseNumber: '', zipCode: '', country: '', city: '' })
  const [paymentCards, setPaymentCards] = useState([])
  const [pcForm, setPcForm] = useState({ cardholderName: '', cardNumber: '', month: 12, year: 30 })
  const [profileStatus, setProfileStatus] = useState('')
  const [showCheckout, setShowCheckout] = useState(false)
  const [checkoutStatus, setCheckoutStatus] = useState('')
  const [selectedAddressId, setSelectedAddressId] = useState(null)
  const [selectedPaymentCardId, setSelectedPaymentCardId] = useState(null)
  const [showOrders, setShowOrders] = useState(false)
  const [orders, setOrders] = useState([])
  const [ordersStatus, setOrdersStatus] = useState('')
  const [paymentStatus, setPaymentStatus] = useState('')

  // Helper to read RFC7807 ProblemDetails errors if present
  const readProblem = async (res) => {
    try {
      const ct = res.headers.get('content-type') || ''
      if (ct.includes('application/problem+json') || ct.includes('application/json')) {
        const data = await res.json()
        if (data?.detail || data?.title) {
          const status = data.status || res.status
          const title = data.title || 'Error'
          const detail = data.detail || ''
          return `${title}${detail ? ': ' + detail : ''} (${status})`
        }
      }
    } catch {}
    return `${res.status} ${res.statusText}`
  }

  useEffect(() => {
    let mounted = true
    fetch('/appsettings.json')
      .then(r => r.json())
      .then(c => { if (mounted) setConfig(c) })
      .catch(() => { if (mounted) setConfig({ ApiBaseUrl: '' }) })
    return () => { mounted = false }
  }, [])

  const apiBase = config?.ApiBaseUrl || ''
  const CRYPTO_PAGE_SIZE = 12
  const cryptoPageCount = Math.max(1, Math.ceil(cryptos.length / CRYPTO_PAGE_SIZE))
  const cryptoPageSafe = Math.min(Math.max(1, cryptoPage), cryptoPageCount)
  const cryptoStart = (cryptoPageSafe - 1) * CRYPTO_PAGE_SIZE
  const cryptoEnd = cryptoStart + CRYPTO_PAGE_SIZE
  const cryptoSlice = cryptos.slice(cryptoStart, cryptoEnd)

  useEffect(() => {
    localStorage.setItem('cryptoPage', String(cryptoPageSafe))
  }, [cryptoPageSafe])

  const fetchCryptos = async () => {
    try {
      const res = await fetch(`${apiBase}/api/cryptocurrencies`, {
        headers: token ? { 'Authorization': `Bearer ${token}` } : {}
      })
      const data = await res.json()
      if (res.ok) setCryptos(data)
    } catch {}
  }

  const fetchExchanges = async (pageNumber = 1) => {
    setExchangesLoading(true)
    setExchangesResult('Loading...')
    try {
      const res = await fetch(`${apiBase}/api/exchanges?pageNumber=${pageNumber}`, {
        headers: { 'Authorization': `Bearer ${token}` }
      })
      let data = null
      try { data = await res.json() } catch { /* ignore json errors */ }
      if (res.ok) {
        const items = data?.Items || data?.items || []
        setExchanges(items)
        setExchangesResult(`OK (${items.length} items)`) 
        setExchangesPage(pageNumber)
      } else {
        setExchangesResult(`${res.status} ${res.statusText}`)
      }
    } catch (err) {
      setExchangesResult(`Error: ${err}`)
    } finally {
      setExchangesLoading(false)
    }
  }

  // Auto-load data when user logs in and config is ready
  useEffect(() => {
    if (token && apiBase) {
      if (activeTab === 'cryptos') {
        setCryptoPage(1)
        fetchCryptos()
      } else if (activeTab === 'exchanges') {
        fetchExchanges(1)
      } else if (activeTab === 'profile') {
        fetchProfile()
        fetchAddresses()
      }
      fetchCart()
    }
  }, [token, apiBase, activeTab])

  const authHeaders = token ? { 'Authorization': `Bearer ${token}` } : {}

  const fetchProfile = async () => {
    try {
      const res = await fetch(`${apiBase}/api/account/me`, { headers: authHeaders })
      if (res.status === 401) { setToken(''); return }
      const data = await res.json().catch(() => ({}))
      if (res.ok && data) setProfile({ email: data.email || '', fullName: data.fullName || '' })
    } catch {}
  }

  // Ensure we have profile info soon after login so we can prefill cardholder name
  useEffect(() => {
    if (token && apiBase) {
      fetchProfile()
    }
  }, [token, apiBase])

  const saveProfile = async () => {
    setProfileStatus('Saving...')
    try {
      const res = await fetch(`${apiBase}/api/account/profile`, {
        method: 'PATCH',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({ fullName: profile.fullName })
      })
      if (res.status === 401) { setToken(''); return }
      setProfileStatus(res.ok ? 'Saved' : `${res.status} ${res.statusText}`)
    } catch (e) {
      setProfileStatus(`Error: ${e}`)
    }
  }

  const fetchAddresses = async () => {
    try {
      const res = await fetch(`${apiBase}/api/addresses`, { headers: authHeaders })
      if (res.status === 401) { setToken(''); return }
      const data = await res.json().catch(() => [])
      if (res.ok && Array.isArray(data)) setAddrList(data)
    } catch {}
  }

  const fetchPaymentCards = async () => {
    try {
      const res = await fetch(`${apiBase}/api/payments`, { headers: authHeaders })
      if (res.status === 401) { setToken(''); return }
      const data = await res.json().catch(() => [])
      if (res.ok && Array.isArray(data)) setPaymentCards(data)
    } catch {}
  }

  const openCheckout = async () => {
    setCheckoutStatus('')
    // Ensure addresses & payment cards are loaded
    if (addrList.length === 0) {
      await fetchAddresses()
    }
    if (paymentCards.length === 0) {
      await fetchPaymentCards()
    }
    if ((addrList?.length ?? 0) > 0 && !selectedAddressId) {
      setSelectedAddressId((addrList[0].id ?? addrList[0].Id))
    }
    if ((paymentCards?.length ?? 0) > 0 && !selectedPaymentCardId) {
      setSelectedPaymentCardId((paymentCards[0].id ?? paymentCards[0].Id))
    }
    setShowCheckout(true)
  }

  const submitCheckout = async (e) => {
    e?.preventDefault?.()
    setCheckoutStatus('Processing…')
    try {
      if (!selectedAddressId) {
        setCheckoutStatus('Please select an address')
        return
      }
      if (!selectedPaymentCardId) {
        setCheckoutStatus('Please select a payment card')
        return
      }
      // Create order using selected address and saved payment card
      const orderRes = await fetch(`${apiBase}/api/orders`, {
        method: 'POST',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({ addressId: selectedAddressId, paymentCardId: selectedPaymentCardId })
      })
      if (orderRes.status === 401) { setToken(''); return }
      if (orderRes.ok) {
        setCheckoutStatus('Order placed successfully')
        setShowCheckout(false)
        await fetchCart()
      } else {
        setCheckoutStatus(`Order failed: ${await readProblem(orderRes)}`)
      }
    } catch (err) {
      setCheckoutStatus(`Error: ${err}`)
    }
  }

  // Add a payment card (used in Profile)
  const addPaymentCard = async (e) => {
    e?.preventDefault?.()
    try {
      const res = await fetch(`${apiBase}/api/payments`, {
        method: 'POST',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          cardholderName: pcForm.cardholderName,
          cardNumber: pcForm.cardNumber,
          month: Number(pcForm.month),
          year: Number(pcForm.year)
        })
      })
      if (res.status === 401) { setToken(''); return }
      if (res.ok || res.status === 201) {
        setPcForm({ cardholderName: '', cardNumber: '', month: 12, year: 30 })
        setPaymentStatus('Saved')
        await fetchPaymentCards()
      } else {
        setPaymentStatus(await readProblem(res))
      }
    } catch {}
  }

  const addAddress = async (e) => {
    e?.preventDefault?.()
    try {
      const res = await fetch(`${apiBase}/api/addresses`, {
        method: 'POST',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          streetName: addrForm.streetName,
          houseNumber: addrForm.houseNumber,
          zipCode: addrForm.zipCode,
          country: addrForm.country,
          city: addrForm.city
        })
      })
      if (res.status === 401) { setToken(''); return }
      if (res.ok || res.status === 201) {
        setAddrForm({ streetName: '', houseNumber: '', zipCode: '', country: '', city: '' })
        fetchAddresses()
      }
    } catch {}
  }

  const deleteAddress = async (id) => {
    try {
      const res = await fetch(`${apiBase}/api/addresses/${id}`, { method: 'DELETE', headers: authHeaders })
      if (res.status === 401) { setToken(''); return }
      if (res.ok || res.status === 204) fetchAddresses()
    } catch {}
  }

  const fetchCart = async () => {
    if (!apiBase || !token) return
    try {
      const res = await fetch(`${apiBase}/api/cart`, { headers: authHeaders })
      if (res.status === 401) { setToken(''); return }
      const data = await res.json().catch(() => [])
      if (res.ok) {
        const arr = Array.isArray(data) ? data : []
        // Keep a stable order: by id ascending. This prevents PATCH updates from jumping items.
        arr.sort((a, b) => {
          const aid = (a.id ?? a.Id ?? 0)
          const bid = (b.id ?? b.Id ?? 0)
          return aid - bid
        })
        setCartItems(arr)
      }
    } catch {}
  }

  // Orders
  const fetchOrders = async () => {
    if (!apiBase || !token) return
    setOrdersStatus('Loading…')
    try {
      const res = await fetch(`${apiBase}/api/orders`, { headers: authHeaders })
      if (res.status === 401) { setToken(''); return }
      const data = await res.json().catch(() => [])
      if (res.ok) {
        setOrders(Array.isArray(data) ? data : [])
        setOrdersStatus(data.length ? `Found ${data.length} order(s)` : 'No orders yet')
      } else {
        setOrdersStatus(`${res.status} ${res.statusText}`)
      }
    } catch (err) {
      setOrdersStatus(`Error: ${err}`)
    }
  }

  const openOrders = async () => {
    await fetchOrders()
    setShowOrders(true)
  }

  const addToCart = async (productIdentifier) => {
    if (!productIdentifier) return
    // Do not duplicate in list; only add if it doesn't exist, always as single unit
    const existing = cartItems.find(i => (i.productIdentifier ?? i.ProductIdentifier) === productIdentifier)
    if (existing) return
    try {
      const res = await fetch(`${apiBase}/api/cart`, {
        method: 'POST',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({ productIdentifier, quantity: 1 })
      })
      if (res.status === 401) { setToken(''); return }
    } catch {}
    await fetchCart()
  }

  const patchQuantity = async (id, newQty) => {
    try {
      const res = await fetch(`${apiBase}/api/cart/${id}`, {
        method: 'PATCH',
        headers: { ...authHeaders, 'Content-Type': 'application/json' },
        body: JSON.stringify({ quantity: Math.max(0, newQty) })
      })
      if (res.status === 401) { setToken(''); return }
    } catch {}
    await fetchCart()
  }

  const removeCartItem = async (id) => {
    try {
      const res = await fetch(`${apiBase}/api/cart/${id}`, { method: 'DELETE', headers: authHeaders })
      if (res.status === 401) { setToken(''); return }
    } catch {}
    await fetchCart()
  }

  // Remove by product identifier convenience for crypto grid cards
  const removeFromCartByPid = async (productIdentifier) => {
    if (!productIdentifier) return
    const existing = cartItems.find(i => (i.productIdentifier ?? i.ProductIdentifier) === productIdentifier)
    if (!existing) return
    await removeCartItem(existing.id ?? existing.Id)
  }

  const clearCart = async () => {
    try {
      const res = await fetch(`${apiBase}/api/cart`, { method: 'DELETE', headers: authHeaders })
      if (res.status === 401) { setToken(''); return }
    } catch {}
    await fetchCart()
  }

  // Grand total across all items
  const grandTotal = cartItems.reduce((sum, it) => {
    const qty = (it.quantity ?? it.Quantity ?? 0)
    const unit = (it.unitPrice ?? it.UnitPrice ?? 0)
    const total = (it.totalPrice ?? it.TotalPrice ?? unit * qty)
    return sum + (typeof total === 'number' ? total : Number(total) || 0)
  }, 0)

  const signOutTop = async () => {
    try {
      const res = await fetch(`${apiBase}/api/account/signout`, {
        method: 'GET',
        headers: { 'Authorization': `Bearer ${token}` }
      })
      if (res.status === 204) {
        setToken('')
        setExchanges([])
        setExchangesPage(1)
        setCryptos([])
      } else {
        // Optional: surface non-204s somewhere; keep silent per minimal UX
      }
    } catch {}
  }
  if (!config) {
    return <div className="app-container"><div className="header"><h1>Cryptocop</h1><p className="status">Loading settings…</p></div></div>
  }

  return (
    <div className={`app-container ${token ? 'with-topbar' : ''}`}>
      {token && (
        <div className="topbar">
          <div className="nav-tabs">
            <button className={`tab-btn ${activeTab === 'cryptos' ? 'active' : ''}`} onClick={() => setActiveTab('cryptos')}>Cryptocurrencies</button>
            <button className={`tab-btn ${activeTab === 'exchanges' ? 'active' : ''}`} onClick={() => setActiveTab('exchanges')}>Exchanges</button>
            <button className={`tab-btn ${activeTab === 'profile' ? 'active' : ''}`} onClick={() => setActiveTab('profile')}>Profile</button>
          </div>
          <button className="signout-btn" onClick={signOutTop} title="GET /api/account/signout">Sign Out</button>
        </div>
      )}

      <div className="header">
        <h1>Cryptocop</h1>
        <p className="status">API base: {apiBase || 'not set'}</p>
      </div>

      {!token ? (
        <div className="auth-center">
          <div className="auth-stack" style={{ width: 'min(900px, 90vw)' }}>
            <Register apiBase={apiBase} onToken={setToken} />
            <SignIn apiBase={apiBase} onToken={setToken} />
          </div>
        </div>
      ) : (
        <div className="content-layout">
          {/* Main content column */}
          <div className="main-col" style={{ display: 'grid', gap: 24 }}>
            {activeTab === 'cryptos' && (
              <div className="card">
                <h2>Cryptocurrencies</h2>
    <button className="icon-btn" aria-label="Refresh cryptocurrencies" title="GET /api/cryptocurrencies"
                        onClick={() => { fetchCryptos(); }}>
                  <svg viewBox="0 0 24 24" aria-hidden="true">
                    <path d="M12 6V3l-4 4 4 4V8c2.76 0 5 2.24 5 5 0 .65-.13 1.27-.36 1.84l1.46 1.46A6.966 6.966 0 0 0 19 13c0-3.87-3.13-7-7-7zm-5.64.16A6.966 6.966 0 0 0 5 11c0 3.87 3.13 7 7 7v3l4-4-4-4v3c-2.76 0-5-2.24-5-5 0-.65.13-1.27.36-1.84L6.36 6.16z"/>
                  </svg>
                </button>
                <div className="toolbar" style={{ flexWrap: 'wrap' }}>
      <button className="btn" onClick={() => setCryptoPage(Math.max(1, cryptoPageSafe - 1))} disabled={cryptoPageSafe <= 1}
        title="GET /api/v2/assets?limit=50&fields=id,slug,symbol,name,metrics/market_data/price_usd">Prev</button>
      <button className="btn" onClick={() => setCryptoPage(Math.min(cryptoPageCount, cryptoPageSafe + 1))} disabled={cryptoPageSafe >= cryptoPageCount}
        title="GET /api/v2/assets?limit=50&fields=id,slug,symbol,name,metrics/market_data/price_usd">Next</button>
                  <span className="status">Page {cryptoPageSafe} of {cryptoPageCount}</span>
                  <span className="status">• {CRYPTO_PAGE_SIZE} per page • Total {cryptos.length}</span>
                </div>
                <div className="crypto-grid">
                  {cryptoSlice.map((c, i) => {
                    const p = c.priceInUsd ?? c.PriceInUsd
                    const pid = (c.slug || c.Slug || c.symbol || c.Symbol || '').toLowerCase()
                    const inCart = cartItems.some(it => (it.productIdentifier ?? it.ProductIdentifier) === pid)
                    const stableKey = String(c.slug || c.Slug || c.symbol || c.Symbol || c.id || i)
                    return (
                      <div className="crypto-card" key={stableKey}>
                        <div className="symbol">{c.symbol}</div>
                        <div className="muted">{c.name}</div>
                        <div style={{ marginTop: 8 }}>
                          {p != null ? (
                            <span title={`GET /api/v2/assets?slugs=${(c.slug || c.Slug || c.symbol || c.Symbol || '').toLowerCase()}&limit=1&fields=metrics/market_data/price_usd`}>${ (typeof p === 'number' ? p : Number(p)).toFixed?.(2) || p }</span>
                          ) : (
                            <span className="muted">price unavailable</span>
                          )}
                        </div>
                        <div style={{ marginTop: 12, display: 'flex', gap: 8 }}>
                          <button className="icon-btn" aria-label="Add to cart" title="POST /api/cart"
                                  onClick={() => addToCart(pid)} disabled={!token || !pid} style={{ position: 'static' }}>
                            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M19 13H13v6h-2v-6H5v-2h6V5h2v6h6v2z"/></svg>
                          </button>
                          <button className="icon-btn danger" aria-label="Remove from cart" title="DELETE /api/cart/{id}"
                                  onClick={() => removeFromCartByPid(pid)} disabled={!token || !inCart} style={{ position: 'static' }}>
                            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9 3h6a1 1 0 0 1 1 1v2h4v2h-1v11a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V8H4V6h4V4a1 1 0 0 1 1-1Zm6 3V5h-6v1h6ZM8 8v11h8V8H8Zm2 2h2v7h-2v-7Zm4 0h2v7h-2v-7Z"/></svg>
                          </button>
                        </div>
                      </div>
                    )
                  })}
                </div>
              </div>
            )}

            {activeTab === 'exchanges' && (
              <div className="card">
                <h2 title="GET /api/exchanges">Exchanges</h2>
                <div className="toolbar">
                  <button className="btn" onClick={() => fetchExchanges(Math.max(1, exchangesPage - 1))} disabled={exchangesLoading || exchangesPage <= 1}
                          title={`GET /api/v1/markets?limit=50&page=${Math.max(1, exchangesPage - 1)}`}>Prev</button>
                  <button className="btn" onClick={() => fetchExchanges(exchangesPage + 1)} disabled={exchangesLoading}
                          title={`GET /api/v1/markets?limit=50&page=${exchangesPage + 1}`}>Next</button>
                  <span className="status">Page {exchangesPage}</span>
                  {exchangesResult && <span className="status">• {exchangesResult}</span>}
                </div>
                <div className="crypto-grid">
                  {exchanges.slice(0, 12).map((e, i) => (
                    <div className="crypto-card" key={`${e.id || e.slug || i}-${i}`}>
                      <div className="symbol">{e.name || e.Name}</div>
                      <div className="muted">{e.assetSymbol ?? e.AssetSymbol ?? ''}</div>
                      {(e.priceInUsd != null || e.PriceInUsd != null) && (
                        <div style={{ marginTop: 8 }}>
                          ${ (e.priceInUsd ?? e.PriceInUsd).toFixed?.(2) || (e.priceInUsd ?? e.PriceInUsd) }
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {activeTab === 'profile' && (
              <div className="card">
                <h2>Profile</h2>
                <div style={{ display: 'grid', gap: 16 }}>
                  <div className="card" style={{ padding: 16 }}>
                    <h3>Account</h3>
                    <div className="field">
                      <label>Email</label>
                      <input value={profile.email} readOnly />
                    </div>
                    <div className="field">
                      <label>Full name</label>
                      <input value={profile.fullName} onChange={e => setProfile(p => ({ ...p, fullName: e.target.value }))} />
                    </div>
                    <button className="btn" onClick={saveProfile} title="PATCH /api/account/profile">Save</button>
                    {profileStatus && <span className="status" style={{ marginLeft: 8 }}>{profileStatus}</span>}
                  </div>

                  <div className="card" style={{ padding: 16 }}>
                    <h3>Addresses</h3>
                    <form onSubmit={addAddress} style={{ display: 'grid', gap: 8 }}>
                      <div className="field"><label>Street</label><input value={addrForm.streetName} onChange={e => setAddrForm(f => ({ ...f, streetName: e.target.value }))} /></div>
                      <div className="field"><label>House number</label><input value={addrForm.houseNumber} onChange={e => setAddrForm(f => ({ ...f, houseNumber: e.target.value }))} /></div>
                      <div className="field"><label>Zip code</label><input value={addrForm.zipCode} onChange={e => setAddrForm(f => ({ ...f, zipCode: e.target.value }))} /></div>
                      <div className="field"><label>Country</label><input value={addrForm.country} onChange={e => setAddrForm(f => ({ ...f, country: e.target.value }))} /></div>
                      <div className="field"><label>City</label><input value={addrForm.city} onChange={e => setAddrForm(f => ({ ...f, city: e.target.value }))} /></div>
                      <button className="btn" type="submit" title="POST /api/addresses">Add address</button>
                    </form>
                    <div style={{ marginTop: 16 }}>
                      {addrList.length === 0 && <div className="muted">No addresses.</div>}
                      <div className="crypto-grid">
                        {addrList.map((a, i) => (
                          <div className="crypto-card" key={`${a.id || a.Id}-${i}`} title="GET /api/addresses">
                            <div style={{ fontWeight: 600 }}>{a.streetName ?? a.StreetName} {a.houseNumber ?? a.HouseNumber}</div>
                            <div className="muted">{a.zipCode ?? a.ZipCode}, {a.city ?? a.City}</div>
                            <div className="muted">{a.country ?? a.Country}</div>
                            <div style={{ marginTop: 8, display: 'flex', justifyContent: 'flex-end' }}>
                              <button className="btn" onClick={() => deleteAddress(a.id ?? a.Id)} title={`DELETE /api/addresses/${a.id ?? a.Id}`}>Delete</button>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>

                  <div className="card" style={{ padding: 16 }}>
                    <h3>Payment cards</h3>
                    <form onSubmit={addPaymentCard} style={{ display: 'grid', gap: 8 }}>
                      <div className="field"><label>Cardholder name</label><input value={pcForm.cardholderName} onChange={e => setPcForm(f => ({ ...f, cardholderName: e.target.value }))} /></div>
                      <div className="field"><label>Card number</label><input value={pcForm.cardNumber} onChange={e => setPcForm(f => ({ ...f, cardNumber: e.target.value }))} placeholder="4111 1111 1111 1111" /></div>
                      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                        <div className="field">
                          <label>Month</label>
                          <input type="number" min={1} max={12} value={pcForm.month} onChange={e => setPcForm(f => ({ ...f, month: e.target.value }))} />
                        </div>
                        <div className="field">
                          <label>Year</label>
                          <input type="number" min={0} max={99} value={pcForm.year} onChange={e => setPcForm(f => ({ ...f, year: e.target.value }))} />
                        </div>
                      </div>
                      <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                        <button className="btn" type="submit" title="POST /api/payments">Add payment card</button>
                        <button type="button" className="icon-btn" onClick={fetchPaymentCards} title="GET /api/payments" aria-label="Refresh payment cards">
                          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 6V3l-4 4 4 4V8c2.76 0 5 2.24 5 5 0 .65-.13 1.27-.36 1.84l1.46 1.46A6.966 6.966 0 0 0 19 13c0-3.87-3.13-7-7-7zm-5.64.16A6.966 6.966 0 0 0 5 11c0 3.87 3.13 7 7 7v3l4-4-4-4v3c-2.76 0-5-2.24-5-5 0-.65.13-1.27.36-1.84L6.36 6.16z"/></svg>
                        </button>
                        {paymentStatus && <span className="status">{paymentStatus}</span>}
                      </div>
                    </form>
                    <div style={{ marginTop: 16 }}>
                      {paymentCards.length === 0 && <div className="muted">No saved cards.</div>}
                      <div className="crypto-grid">
                        {paymentCards.map((c, i) => (
                          <div className="crypto-card" key={`${c.id || c.Id}-${i}`} title="GET /api/payments">
                            <div style={{ fontWeight: 600 }}>{c.cardholderName ?? c.CardholderName}</div>
                            <div className="muted">{c.cardNumber ?? c.CardNumber}</div>
                            <div className="muted">Exp: {(c.month ?? c.Month)?.toString().padStart(2, '0')}/{c.year ?? c.Year}</div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Cart column */}
          <div className="cart-panel">
            <div className="card">
              <h2>Shopping Cart</h2>
              <button className="icon-btn" aria-label="Refresh cart" title="GET /api/cart" onClick={fetchCart}>
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  <path d="M12 6V3l-4 4 4 4V8c2.76 0 5 2.24 5 5 0 .65-.13 1.27-.36 1.84l1.46 1.46A6.966 6.966 0 0 0 19 13c0-3.87-3.13-7-7-7zm-5.64.16A6.966 6.966 0 0 0 5 11c0 3.87 3.13 7 7 7v3l4-4-4-4v3c-2.76 0-5-2.24-5-5 0-.65.13-1.27.36-1.84L6.36 6.16z"/>
                </svg>
              </button>
              <button className="icon-btn danger" style={{ right: 48 }} aria-label="Clear cart" title="DELETE /api/cart"
                      onClick={clearCart} disabled={cartItems.length === 0}>
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  <path d="M9 3h6a1 1 0 0 1 1 1v2h4v2h-1v11a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V8H4V6h4V4a1 1 0 0 1 1-1Zm6 3V5h-6v1h6ZM8 8v11h8V8H8Zm2 2h2v7h-2v-7Zm4 0h2v7h-2v-7Z"/>
                </svg>
              </button>
              <div className="toolbar" style={{ justifyContent: 'space-between', flexWrap: 'wrap' }}>
                <span className="status">Items: {cartItems.length}</span>
                <div style={{ display: 'flex', gap: 8 }}>
                  <button className="btn" onClick={openOrders} title="GET /api/orders">View pending orders</button>
                  <button className="btn" onClick={openCheckout} disabled={cartItems.length === 0} title="POST /api/orders">Go to checkout</button>
                </div>
              </div>
              <div className="cart-items">
                {cartItems.length === 0 && <div className="muted">Your cart is empty.</div>}
                {cartItems.map((it, idx) => {
                  const id = it.id ?? it.Id
                  const pid = it.productIdentifier ?? it.ProductIdentifier
                  const qty = it.quantity ?? it.Quantity
                  const unit = it.unitPrice ?? it.UnitPrice
                  const total = it.totalPrice ?? it.TotalPrice ?? (unit && qty ? unit * qty : undefined)
                  return (
                    <div key={`${id}`} className="crypto-card cart-card">
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 8 }}>
                        <div>
                          <div className="symbol">{pid}</div>
                          <div className="muted" style={{ marginTop: 2 }}>ID: {id}</div>
                        </div>
                        <button className="icon-btn danger" aria-label="Remove item" title={`DELETE /api/cart/${id}`}
                                onClick={() => removeCartItem(id)} style={{ position: 'static' }}>
                          <svg viewBox="0 0 24 24" aria-hidden="true">
                            <path d="M9 3h6a1 1 0 0 1 1 1v2h4v2h-1v11a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V8H4V6h4V4a1 1 0 0 1 1-1Zm6 3V5h-6v1h6ZM8 8v11h8V8H8Zm2 2h2v7h-2v-7Zm4 0h2v7h-2v-7Z"/>
                          </svg>
                        </button>
                      </div>
                      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6, marginTop: 8 }}>
                        <div className="muted">Qty</div>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                          <button className="icon-btn" aria-label="Decrease" title={`PATCH /api/cart/${id}`}
                                  onClick={() => patchQuantity(id, (qty ?? 0) - 1)} style={{ position: 'static' }}>
                            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M19 13H5v-2h14v2z"/></svg>
                          </button>
                          <span>{qty}</span>
                          <button className="icon-btn" aria-label="Increase" title={`PATCH /api/cart/${id}`}
                                  onClick={() => patchQuantity(id, (qty ?? 0) + 1)} style={{ position: 'static' }}>
                            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M19 13H13v6h-2v-6H5v-2h6V5h2v6h6v2z"/></svg>
                          </button>
                        </div>
                        <div className="muted">Unit</div>
                        <div>{unit != null ? `$${unit.toFixed?.(2) || unit}` : '-'}</div>
                        <div className="muted">Total</div>
                        <div>{total != null ? `$${total.toFixed?.(2) || total}` : '-'}</div>
                      </div>
                    </div>
                  )
                })}
              </div>
              {/* Grand total row */}
              <div style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid var(--border)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 600 }}>
                  <span>Total price</span>
                  <span>${grandTotal.toFixed(2)}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Checkout Modal */}
      {showCheckout && (
        <div className="modal-backdrop" onClick={() => setShowCheckout(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>Checkout</h3>
            <button className="icon-btn" aria-label="Refresh checkout" title="GET /api/addresses, GET /api/payments"
                    onClick={async () => { setCheckoutStatus(''); await fetchAddresses(); await fetchPaymentCards(); }}>
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M12 6V3l-4 4 4 4V8c2.76 0 5 2.24 5 5 0 .65-.13 1.27-.36 1.84l1.46 1.46A6.966 6.966 0 0 0 19 13c0-3.87-3.13-7-7-7zm-5.64.16A6.966 6.966 0 0 0 5 11c0 3.87 3.13 7 7 7v3l4-4-4-4v3c-2.76 0-5-2.24-5-5 0-.65.13-1.27.36-1.84L6.36 6.16z"/>
              </svg>
            </button>
            <form onSubmit={submitCheckout} className="checkout-form">
              <div className="field">
                <label>Address</label>
                {addrList.length === 0 ? (
                  <div className="muted">No addresses found. Please add one in the Profile tab.</div>
                ) : (
                  <select value={selectedAddressId ?? ''} onChange={e => setSelectedAddressId(Number(e.target.value))}>
                    {addrList.map(a => (
                      <option key={a.id ?? a.Id} value={a.id ?? a.Id}>
                        {(a.streetName ?? a.StreetName)} {(a.houseNumber ?? a.HouseNumber)}, {(a.city ?? a.City)} {(a.zipCode ?? a.ZipCode)}
                      </option>
                    ))}
                  </select>
                )}
              </div>
              <div className="field">
                <label>Payment card</label>
                {paymentCards.length === 0 ? (
                  <div className="muted">No saved payment cards. Add one in the Profile tab.</div>
                ) : (
                  <select
                    title="Select a saved card (GET /api/payments). Checkout will POST /api/orders with paymentCardId"
                    value={selectedPaymentCardId ?? ''}
                    onChange={e => setSelectedPaymentCardId(Number(e.target.value))}
                  >
                    {paymentCards.map(p => (
                      <option key={p.id ?? p.Id} value={p.id ?? p.Id}>
                        {(p.cardholderName ?? p.CardholderName)} — {(p.cardNumber ?? p.CardNumber)} (exp {(p.month ?? p.Month)?.toString().padStart(2,'0')}/{p.year ?? p.Year})
                      </option>
                    ))}
                  </select>
                )}
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, marginTop: 12 }}>
                <button type="button" className="btn" onClick={() => setShowCheckout(false)}>Cancel</button>
                <button type="submit" className="btn" disabled={addrList.length === 0 || cartItems.length === 0} title="POST /api/orders">Place order</button>
              </div>
              {checkoutStatus && <div className="status" style={{ marginTop: 8 }}>{checkoutStatus}</div>}
            </form>
          </div>
        </div>
      )}

      {/* Orders Modal */}
      {showOrders && (
        <div className="modal-backdrop" onClick={() => setShowOrders(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>Your Orders</h3>
            <div className="status" style={{ marginBottom: 8 }}>{ordersStatus}</div>
            {orders.length === 0 ? (
              <div className="muted">No orders to show.</div>
            ) : (
              <div style={{ display: 'grid', gap: 12, maxHeight: '60vh', overflow: 'auto' }}>
                {orders.map((o, idx) => {
                  const id = o.id ?? o.Id
                  const date = o.orderDate ?? o.OrderDate
                  const total = o.totalPrice ?? o.TotalPrice
                  const items = o.orderItems ?? o.OrderItems ?? []
                  const card = o.creditCard ?? o.CreditCard
                  return (
                    <div className="crypto-card" key={`${id}-${idx}`}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <div style={{ fontWeight: 700 }}>Order #{id}</div>
                        <div className="muted">{date}</div>
                      </div>
                      <div style={{ marginTop: 6, display: 'flex', justifyContent: 'space-between' }}>
                        <div className="muted">Items: {items.length}</div>
                        <div><strong>Total:</strong> ${typeof total === 'number' ? total.toFixed(2) : total}</div>
                      </div>
                      {card && (
                        <div className="muted" style={{ marginTop: 4 }}>Card: {card}</div>
                      )}
                      <div style={{ marginTop: 8 }}>
                        <ul style={{ margin: 0, paddingLeft: 18 }}>
                          {items.map((it, ii) => (
                            <li key={`${id}-item-${ii}`}>
                              {(it.productIdentifier ?? it.ProductIdentifier)} × {(it.quantity ?? it.Quantity)} — $
                              {(() => { const t = it.totalPrice ?? it.TotalPrice; return typeof t === 'number' ? t.toFixed(2) : t })()}
                            </li>
                          ))}
                        </ul>
                      </div>
                    </div>
                  )
                })}
              </div>
            )}
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, marginTop: 12 }}>
              <button className="btn" onClick={() => setShowOrders(false)}>Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
