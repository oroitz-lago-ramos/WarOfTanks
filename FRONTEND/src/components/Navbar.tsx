import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

const Brand = () => (
  <div className="flex items-center gap-3 font-mono text-[13px] font-bold tracking-[2.34px] text-[#e7ecef]">
    <span className="relative grid size-[22px] shrink-0 place-items-center border border-[#e7ecef]">
      <span className="size-2.5 border border-[#e7ecef]" />
      <span className="absolute size-[5px] rounded-full bg-[#5ebc7b]" />
    </span>
    <span>WAR OF TANKS</span>
  </div>
)

const Navbar = () => {
  const { player, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = async () => {
    await logout()
    navigate('/login')

    
  }

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `font-mono text-[11px] tracking-[1.1px] uppercase transition-colors ${
      isActive ? 'text-[#e7ecef]' : 'text-[#98a1ad] hover:text-[#e7ecef]'
    }`

  return (
    <header className="border-b border-[#2a313b] bg-[#0e1116]">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-6 py-4">
        <Brand />
        <nav className="flex items-center gap-7">
          <NavLink to="/leaderboard" className={linkClass}>Leaderboard</NavLink>
          <NavLink to="/stats" className={linkClass}>Stats</NavLink>
          <NavLink to="/history" className={linkClass}>History</NavLink>
        </nav>
        <div className="flex items-center gap-5">
          <span className="font-mono text-[11px] tracking-[1px] text-[#98a1ad]">
            {player?.username}
          </span>
          <button
            onClick={handleLogout}
            className="font-mono text-[11px] tracking-[1.1px] uppercase text-[#98a1ad] transition-colors hover:text-[#ee6951]"
          >
            Logout
          </button>
        </div>
      </div>
    </header>
  )
}

export default Navbar
