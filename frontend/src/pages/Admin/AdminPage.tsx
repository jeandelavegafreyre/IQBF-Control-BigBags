import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../../context/AuthContext'
import { getActiveShips } from '../../services/shipService'
import { createBL, createProduct, createShip, getProducts, getUsers, updateUserRole } from '../../services/adminService'
import type { Ship } from '../../types/ships'
import type { Product, UserSummary } from '../../types/admin'
import './AdminPage.css'

function errorMessage(error: unknown) {
  if (axios.isAxiosError(error)) return String(error.response?.data?.error ?? error.message)
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
  const [ships, setShips] = useState<Ship[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [users, setUsers] = useState<UserSummary[]>([])
  const [shipName, setShipName] = useState('')
  const [productName, setProductName] = useState('')
  const [bl, setBL] = useState({ code: '', totalQuantity: '', shipId: '', productId: '' })
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  async function load() {
    try {
      const [shipData, productData, userData] = await Promise.all([getActiveShips(), getProducts(), getUsers()])
      setShips(shipData); setProducts(productData); setUsers(userData)
      setBL(current => ({ ...current, shipId: current.shipId || shipData[0]?.id || '', productId: current.productId || productData[0]?.id || '' }))
    } catch (e) { setError(errorMessage(e)) }
  }
  useEffect(() => { void load() }, [])

  async function submitShip(e: FormEvent) {
    e.preventDefault(); setError(''); setMessage('')
    try { await createShip(shipName); setShipName(''); setMessage('Nave creada correctamente.'); await load() } catch (x) { setError(errorMessage(x)) }
  }
  async function submitProduct(e: FormEvent) {
    e.preventDefault(); setError(''); setMessage('')
    try { await createProduct(productName); setProductName(''); setMessage('Producto creado correctamente.'); await load() } catch (x) { setError(errorMessage(x)) }
  }
  async function submitBL(e: FormEvent) {
    e.preventDefault(); setError(''); setMessage('')
    const quantity=Number(bl.totalQuantity)
    if (!Number.isFinite(quantity) || quantity < 0) { setError('La cantidad declarada debe ser un número válido mayor o igual a cero.'); return }
    try { await createBL(bl.code, quantity, bl.shipId, bl.productId); setBL(c=>({...c,code:'',totalQuantity:''})); setMessage('BL creado correctamente.') } catch (x) { setError(errorMessage(x)) }
  }
  async function changeRole(item: UserSummary, role: number) {
    setError(''); setMessage('')
    try { await updateUserRole(item.id, role); setMessage(`Rol de ${item.uid} actualizado.`); await load() } catch (x) { setError(errorMessage(x)) }
  }

  if (user?.role !== 'Administrator') return <main className="admin-shell"><p>Acceso exclusivo para Administradores.</p><button onClick={()=>navigate('/ships')}>Volver</button></main>

  return <main className="admin-shell">
    <header className="admin-header"><div><span className="eyebrow">IQBF Control</span><h1>Administración</h1></div><button className="secondary-action" onClick={()=>navigate('/ships')}>Volver a operación</button></header>
    {error ? <p className="operations-message operations-error">{error}</p> : null}
    {message ? <p className="operations-message operations-success">{message}</p> : null}
    <div className="admin-grid">
      <section className="admin-card"><h2>Naves</h2><form onSubmit={submitShip}><label>Nombre<input value={shipName} onChange={e=>setShipName(e.target.value)} required /></label><button type="submit">Crear nave</button></form><p>{ships.length} nave(s) activa(s)</p></section>
      <section className="admin-card"><h2>Productos</h2><form onSubmit={submitProduct}><label>Nombre<input value={productName} onChange={e=>setProductName(e.target.value)} required /></label><button type="submit">Crear producto</button></form><ul>{products.map(p=><li key={p.id}>{p.name}</li>)}</ul></section>
      <section className="admin-card"><h2>BL</h2><form onSubmit={submitBL}>
        <label>Código<input value={bl.code} onChange={e=>setBL(c=>({...c,code:e.target.value}))} required /></label>
        <label>Cantidad declarada<input type="number" min="0" step="0.001" value={bl.totalQuantity} onChange={e=>setBL(c=>({...c,totalQuantity:e.target.value}))} required /></label>
        <label>Nave<select value={bl.shipId} onChange={e=>setBL(c=>({...c,shipId:e.target.value}))} required>{ships.map(s=><option key={s.id} value={s.id}>{s.name}</option>)}</select></label>
        <label>Producto<select value={bl.productId} onChange={e=>setBL(c=>({...c,productId:e.target.value}))} required>{products.filter(p=>p.isActive).map(p=><option key={p.id} value={p.id}>{p.name}</option>)}</select></label>
        <button type="submit">Crear BL</button></form></section>
      <section className="admin-card admin-users"><h2>Usuarios</h2>{users.map(u=><div className="admin-user" key={u.id}><span><strong>{u.uid}</strong> · {u.fullName}</span><select aria-label={`Rol de ${u.uid}`} value={roleValue(u.role)} onChange={e=>void changeRole(u,Number(e.target.value))}><option value={1}>Administrator</option><option value={2}>Yard</option><option value={3}>User</option></select><small>{roleName(u.role)} · {u.isActive?'Activo':'Inactivo'}</small></div>)}</section>
    </div>
  </main>
}
