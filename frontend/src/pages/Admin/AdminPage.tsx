import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../../context/AuthContext'
import { getActiveShips } from '../../services/shipService'
import { getActiveBLsByShip } from '../../services/blService'
import { createBL, createProduct, createShip, createUser, getProducts, getUsers, resetUserPassword, updateUserRole, updateUserStatus } from '../../services/adminService'
import type { Ship } from '../../types/ships'
import type { BL } from '../../types/bls'
import type { Product, UserSummary } from '../../types/admin'
import './AdminPage.css'

type AdminSection = 'overview' | 'ships' | 'products' | 'bls' | 'users'

const menuItems: Array<{ id: AdminSection; label: string; short: string; description: string }> = [
  { id: 'overview', label: 'Resumen', short: 'RS', description: 'Vista general de configuración' },
  { id: 'ships', label: 'Naves', short: 'NV', description: 'Gestiona naves operativas' },
  { id: 'products', label: 'Productos', short: 'PR', description: 'Catálogo de productos IQBF' },
  { id: 'bls', label: 'BL', short: 'BL', description: 'Configura BL y cantidades' },
  { id: 'users', label: 'Usuarios', short: 'US', description: 'Accesos, roles y contraseñas' },
]

function errorMessage(error: unknown) {
  if (axios.isAxiosError(error)) return String(error.response?.data?.error ?? error.response?.data?.message ?? error.message)
  return 'Ocurrió un error inesperado.'
}

function roleName(role: string | number) {
  if (role === 1 || role === 'Administrator') return 'Administrator'
  if (role === 2 || role === 'Yard') return 'Yard'
  return 'User'
}

function roleValue(role: string | number) {
  if (role === 1 || role === 'Administrator') return 1
  if (role === 2 || role === 'Yard') return 2
  return 3
}

export function AdminPage() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const [section, setSection] = useState<AdminSection>('overview')
  const [ships, setShips] = useState<Ship[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [users, setUsers] = useState<UserSummary[]>([])
  const [bls, setBLs] = useState<BL[]>([])
  const [shipName, setShipName] = useState('')
  const [productName, setProductName] = useState('')
  const [bl, setBL] = useState({ code: '', totalQuantity: '', shipId: '', productId: '' })
  const [newUser, setNewUser] = useState({ uid: '', firstName: '', lastName: '', password: '', role: 3 })
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [resetUserId, setResetUserId] = useState('')
  const [resetPassword, setResetPassword] = useState('')

  async function loadBLs(shipId: string) {
    if (!shipId) {
      setBLs([])
      return
    }
    try {
      setBLs(await getActiveBLsByShip(shipId))
    } catch (e) {
      setError(errorMessage(e))
      setBLs([])
    }
  }

  async function load() {
    try {
      const [shipData, productData, userData] = await Promise.all([getActiveShips(), getProducts(), getUsers()])
      setShips(shipData)
      setProducts(productData)
      setUsers(userData)
      const selectedShipId = bl.shipId || shipData[0]?.id || ''
      setBL((current) => ({
        ...current,
        shipId: current.shipId || selectedShipId,
        productId: current.productId || productData[0]?.id || '',
      }))
      await loadBLs(selectedShipId)
    } catch (e) {
      setError(errorMessage(e))
    }
  }

  useEffect(() => { void load() }, [])

  async function submitShip(e: FormEvent) {
    e.preventDefault(); setError(''); setMessage('')
    try { await createShip(shipName); setShipName(''); setMessage('Nave creada correctamente.'); await load() }
    catch (x) { setError(errorMessage(x)) }
  }

  async function submitProduct(e: FormEvent) {
    e.preventDefault(); setError(''); setMessage('')
    try { await createProduct(productName); setProductName(''); setMessage('Producto creado correctamente.'); await load() }
    catch (x) { setError(errorMessage(x)) }
  }

  async function submitBL(e: FormEvent) {
    e.preventDefault(); setError(''); setMessage('')
    const quantity = Number(bl.totalQuantity)
    if (!Number.isInteger(quantity) || quantity <= 0) {
      setError('La cantidad declarada debe ser un número entero mayor que cero.')
      return
    }
    try {
      await createBL(bl.code, quantity, bl.shipId, bl.productId)
      setBL((current) => ({ ...current, code: '', totalQuantity: '' }))
      setMessage('BL creado correctamente.')
      await loadBLs(bl.shipId)
    } catch (x) { setError(errorMessage(x)) }
  }

  async function submitUser(e: FormEvent) {
    e.preventDefault(); setError(''); setMessage('')
    if (newUser.password.length < 8) { setError('La contraseña debe tener al menos 8 caracteres.'); return }
    try {
      await createUser(newUser.uid, newUser.firstName, newUser.lastName, newUser.password, newUser.role)
      setNewUser({ uid: '', firstName: '', lastName: '', password: '', role: 3 })
      setMessage('Usuario creado correctamente.')
      await load()
    } catch (x) { setError(errorMessage(x)) }
  }

  async function changeRole(item: UserSummary, role: number) {
    setError(''); setMessage('')
    if (item.uid === user?.uid && role !== 1) { setError('No puedes quitarte tu propio rol de Administrador durante una sesión activa.'); return }
    try { await updateUserRole(item.id, role); setMessage(`Rol de ${item.uid} actualizado.`); await load() }
    catch (x) { setError(errorMessage(x)) }
  }

  async function changeStatus(item: UserSummary) {
    setError(''); setMessage('')
    if (item.uid === user?.uid && item.isActive) { setError('No puedes desactivar tu propia cuenta durante una sesión activa.'); return }
    const nextStatus = !item.isActive
    try { await updateUserStatus(item.id, nextStatus); setMessage(`${item.uid} ${nextStatus ? 'activado' : 'desactivado'} correctamente.`); await load() }
    catch (x) { setError(errorMessage(x)) }
  }

  async function submitPasswordReset(e: FormEvent, item: UserSummary) {
    e.preventDefault(); setError(''); setMessage('')
    if (resetPassword.length < 8) { setError('La contraseña debe tener al menos 8 caracteres.'); return }
    try {
      await resetUserPassword(item.id, resetPassword)
      setResetPassword(''); setResetUserId('')
      setMessage(`Contraseña de ${item.uid} restablecida correctamente.`)
    } catch (x) { setError(errorMessage(x)) }
  }

  function changeBLShip(shipId: string) {
    setBL((current) => ({ ...current, shipId }))
    setError('')
    void loadBLs(shipId)
  }

  if (user?.role !== 'Administrator') {
    return <main className="admin-shell"><p>Acceso exclusivo para Administradores.</p><button onClick={() => navigate('/ships')}>Volver</button></main>
  }

  const activeProducts = products.filter((product) => product.isActive)
  const activeUsers = users.filter((item) => item.isActive)

  return (
    <main className="admin-shell">
      <header className="admin-header">
        <div>
          <span className="eyebrow">IQBF Control</span>
          <h1>Configuración</h1>
          <p>Administra los catálogos y accesos de la operación.</p>
        </div>
        <button className="secondary-action" onClick={() => navigate('/ships')}>Volver a operación</button>
      </header>

      {error ? <p className="operations-message operations-error">{error}</p> : null}
      {message ? <p className="operations-message operations-success">{message}</p> : null}

      <div className="admin-layout">
        <aside className="admin-sidebar" aria-label="Menú de configuración">
          <div className="admin-sidebar-title">Configuración</div>
          <nav className="admin-nav">
            {menuItems.map((item) => (
              <button
                type="button"
                key={item.id}
                className={`admin-nav-item${section === item.id ? ' is-active' : ''}`}
                onClick={() => setSection(item.id)}
              >
                <span className="admin-nav-icon">{item.short}</span>
                <span className="admin-nav-copy"><strong>{item.label}</strong><small>{item.description}</small></span>
                <span className="admin-nav-arrow">›</span>
              </button>
            ))}
          </nav>
          <div className="admin-sidebar-user">
            <span>Sesión</span>
            <strong>{user.fullName}</strong>
            <small>Administrador</small>
          </div>
        </aside>

        <div className="admin-content">
          {section === 'overview' ? (
            <section className="admin-panel">
              <div className="admin-panel-heading"><div><span className="eyebrow">Panel general</span><h2>Resumen de configuración</h2><p>Acceso rápido a los principales catálogos del sistema.</p></div></div>
              <div className="admin-summary-grid">
                <button type="button" className="admin-summary-card" onClick={() => setSection('ships')}><span className="admin-summary-icon">NV</span><div><strong>{ships.length}</strong><span>Naves activas</span><small>Gestionar naves</small></div></button>
                <button type="button" className="admin-summary-card" onClick={() => setSection('products')}><span className="admin-summary-icon">PR</span><div><strong>{activeProducts.length}</strong><span>Productos activos</span><small>Gestionar productos</small></div></button>
                <button type="button" className="admin-summary-card" onClick={() => setSection('bls')}><span className="admin-summary-icon">BL</span><div><strong>{bls.length}</strong><span>BL de nave seleccionada</span><small>Gestionar BL</small></div></button>
                <button type="button" className="admin-summary-card" onClick={() => setSection('users')}><span className="admin-summary-icon">US</span><div><strong>{activeUsers.length}</strong><span>Usuarios activos</span><small>Gestionar accesos</small></div></button>
              </div>
              <div className="admin-info-card"><span className="admin-info-badge">Administración</span><div><h3>Configuración centralizada</h3><p>Los cambios realizados aquí afectan los catálogos disponibles durante la recepción y el despacho.</p></div></div>
            </section>
          ) : null}

          {section === 'ships' ? (
            <section className="admin-panel">
              <div className="admin-panel-heading"><div><span className="eyebrow">Catálogo</span><h2>Naves</h2><p>Crea y consulta las naves disponibles para iniciar una operación.</p></div><span className="admin-count-badge">{ships.length} activas</span></div>
              <div className="admin-two-column">
                <div className="admin-form-card"><h3>Nueva nave</h3><form onSubmit={submitShip}><label>Nombre de la nave<input value={shipName} onChange={(e) => setShipName(e.target.value)} placeholder="Ej. NAVE IQBF 01" required /></label><button type="submit">Crear nave</button></form></div>
                <div className="admin-list-card"><h3>Naves activas</h3>{ships.length === 0 ? <p className="admin-empty">No hay naves activas.</p> : <div className="admin-simple-list">{ships.map((ship) => <div className="admin-simple-row" key={ship.id}><span className="status-dot" /><strong>{ship.name}</strong><span className="admin-state active">Activa</span></div>)}</div>}</div>
              </div>
            </section>
          ) : null}

          {section === 'products' ? (
            <section className="admin-panel">
              <div className="admin-panel-heading"><div><span className="eyebrow">Catálogo</span><h2>Productos</h2><p>Administra los productos asociados a cada BL.</p></div><span className="admin-count-badge">{activeProducts.length} activos</span></div>
              <div className="admin-two-column">
                <div className="admin-form-card"><h3>Nuevo producto</h3><form onSubmit={submitProduct}><label>Nombre del producto<input value={productName} onChange={(e) => setProductName(e.target.value)} placeholder="Ej. Nitrato de amonio" required /></label><button type="submit">Crear producto</button></form></div>
                <div className="admin-list-card"><h3>Productos registrados</h3>{products.length === 0 ? <p className="admin-empty">No hay productos registrados.</p> : <div className="admin-simple-list">{products.map((product) => <div className="admin-simple-row" key={product.id}><span className={`status-dot${product.isActive ? '' : ' inactive'}`} /><strong>{product.name}</strong><span className={`admin-state ${product.isActive ? 'active' : 'inactive'}`}>{product.isActive ? 'Activo' : 'Inactivo'}</span></div>)}</div>}</div>
              </div>
            </section>
          ) : null}

          {section === 'bls' ? (
            <section className="admin-panel">
              <div className="admin-panel-heading"><div><span className="eyebrow">Operación</span><h2>BL</h2><p>Registra el BL, producto, nave y cantidad declarada de Big Bags.</p></div><span className="admin-count-badge">{bls.length} registrados</span></div>
              <div className="admin-form-card admin-form-wide">
                <h3>Nuevo BL</h3>
                <form className="admin-bl-form" onSubmit={submitBL}>
                  <label>Código<input value={bl.code} onChange={(e) => setBL((current) => ({ ...current, code: e.target.value }))} placeholder="Ej. BL-001" required /></label>
                  <label>Cantidad declarada (Big Bags)<input type="number" min="1" step="1" inputMode="numeric" value={bl.totalQuantity} onChange={(e) => setBL((current) => ({ ...current, totalQuantity: e.target.value }))} required /></label>
                  <label>Nave<select value={bl.shipId} onChange={(e) => changeBLShip(e.target.value)} required>{ships.map((ship) => <option key={ship.id} value={ship.id}>{ship.name}</option>)}</select></label>
                  <label>Producto<select value={bl.productId} onChange={(e) => setBL((current) => ({ ...current, productId: e.target.value }))} required>{activeProducts.map((product) => <option key={product.id} value={product.id}>{product.name}</option>)}</select></label>
                  <button type="submit">Crear BL</button>
                </form>
              </div>
              <div className="admin-list-card admin-bl-list">
                <div className="admin-list-heading"><h3>BL registrados</h3><label>Nave<select value={bl.shipId} onChange={(e) => changeBLShip(e.target.value)}>{ships.map((ship) => <option key={ship.id} value={ship.id}>{ship.name}</option>)}</select></label></div>
                {bls.length === 0 ? <p className="admin-empty">No hay BL registrados para esta nave.</p> : <div className="admin-table-wrap"><table className="admin-table"><thead><tr><th>Código</th><th>Producto</th><th>Big Bags declarados</th><th>Estado</th></tr></thead><tbody>{bls.map((item) => <tr key={item.id}><td><strong>{item.code}</strong></td><td>{item.productName}</td><td>{item.totalQuantity.toLocaleString('es-PE')}</td><td><span className={`admin-state ${item.isActive ? 'active' : 'inactive'}`}>{item.isActive ? 'Activo' : 'Inactivo'}</span></td></tr>)}</tbody></table></div>}
              </div>
            </section>
          ) : null}

          {section === 'users' ? (
            <section className="admin-panel">
              <div className="admin-panel-heading"><div><span className="eyebrow">Seguridad</span><h2>Usuarios</h2><p>Crea usuarios y administra roles, estado y contraseñas.</p></div><span className="admin-count-badge">{activeUsers.length} activos</span></div>
              <div className="admin-form-card admin-form-wide"><h3>Nuevo usuario</h3><form className="admin-user-form" onSubmit={submitUser}><label>UID<input value={newUser.uid} onChange={(e) => setNewUser((current) => ({ ...current, uid: e.target.value }))} maxLength={50} autoComplete="off" required /></label><label>Nombre<input value={newUser.firstName} onChange={(e) => setNewUser((current) => ({ ...current, firstName: e.target.value }))} required /></label><label>Apellido<input value={newUser.lastName} onChange={(e) => setNewUser((current) => ({ ...current, lastName: e.target.value }))} required /></label><label>Contraseña temporal<input type="password" value={newUser.password} onChange={(e) => setNewUser((current) => ({ ...current, password: e.target.value }))} minLength={8} autoComplete="new-password" required /></label><label>Rol<select value={newUser.role} onChange={(e) => setNewUser((current) => ({ ...current, role: Number(e.target.value) }))}><option value={3}>User</option><option value={2}>Yard</option><option value={1}>Administrator</option></select></label><button type="submit">Crear usuario</button></form></div>
              <div className="admin-users-list">
                {users.map((item) => (
                  <article className="admin-user-card" key={item.id}>
                    <div className="admin-user-main"><div className="admin-user-avatar">{item.fullName?.trim().charAt(0).toUpperCase() || item.uid.charAt(0).toUpperCase()}</div><div><strong>{item.fullName}</strong><span>{item.uid}</span><small>{roleName(item.role)} · {item.isActive ? 'Activo' : 'Inactivo'}{item.uid === user?.uid ? ' · sesión actual' : ''}</small></div></div>
                    <div className="admin-user-actions"><select aria-label={`Rol de ${item.uid}`} value={roleValue(item.role)} disabled={item.uid === user?.uid} onChange={(e) => void changeRole(item, Number(e.target.value))}><option value={1}>Administrator</option><option value={2}>Yard</option><option value={3}>User</option></select><button type="button" className="secondary-action" disabled={item.uid === user?.uid} onClick={() => void changeStatus(item)}>{item.isActive ? 'Desactivar' : 'Activar'}</button><button type="button" className="secondary-action" onClick={() => { setResetUserId(resetUserId === item.id ? '' : item.id); setResetPassword(''); setError(''); setMessage('') }}>{resetUserId === item.id ? 'Cancelar' : 'Restablecer contraseña'}</button></div>
                    {resetUserId === item.id ? <form className="admin-reset-form" onSubmit={(e) => void submitPasswordReset(e, item)}><label>Nueva contraseña temporal<input type="password" value={resetPassword} onChange={(e) => setResetPassword(e.target.value)} minLength={8} autoComplete="new-password" required /></label><button type="submit">Guardar nueva contraseña</button></form> : null}
                  </article>
                ))}
              </div>
            </section>
          ) : null}
        </div>
      </div>
    </main>
  )
}
