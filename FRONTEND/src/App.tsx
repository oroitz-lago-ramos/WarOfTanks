import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
  Outlet,
} from 'react-router-dom'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import LeaderboardPage from './pages/LeaderboardPage'
import StatsPage from './pages/StatsPage'
import HistoryPage from './pages/HistoryPage'
import GamePage from './pages/GamePage'
import { useAuth } from './hooks/useAuth'
import Navbar from './components/Navbar'

const AppLoader = () => (
  <div className="bg-bg grid min-h-svh place-items-center">
    <span className="text-dim font-mono text-[11px] tracking-[2px] uppercase">
      Loading…
    </span>
  </div>
)

const RootRedirect = () => {
  const { accessToken, initializing } = useAuth()
  if (initializing) return <AppLoader />
  return accessToken ? (
    <Navigate to="/leaderboard" replace />
  ) : (
    <Navigate to="/login" replace />
  )
}

const ProtectedLayout = () => {
  const { accessToken, initializing } = useAuth()
  if (initializing) return <AppLoader />
  if (!accessToken) return <Navigate to="/login" replace />
  return (
    <div className="bg-bg text-fg min-h-screen text-left">
      <Navbar />
      <main>
        <Outlet />
      </main>
    </div>
  )
}

const App = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<RootRedirect />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route element={<ProtectedLayout />}>
          <Route path="/leaderboard" element={<LeaderboardPage />} />
          <Route path="/stats" element={<StatsPage />} />
          <Route path="/history" element={<HistoryPage />} />
          <Route path="/play" element={<GamePage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
