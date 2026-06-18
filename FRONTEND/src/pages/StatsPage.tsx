import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { client } from '../api/client'
import { useAuth } from '../hooks/useAuth'
import type { Match, Player } from '../types'
import { formatDateTime, formatDuration } from '../utils/format'
import PageContainer from '../components/ui/PageContainer'
import PageHeader from '../components/ui/PageHeader'
import Panel from '../components/ui/Panel'
import Button from '../components/ui/Button'
import StatCard from '../components/ui/StatCard'
import Scoreboard from '../components/ui/Scoreboard'
import ResultBadge from '../components/ui/ResultBadge'
import Eyebrow from '../components/ui/Eyebrow'
import SkeletonRows from '../components/ui/SkeletonRows'
import ErrorBanner from '../components/ui/ErrorBanner'

const StatsPage = () => {
  const navigate = useNavigate()
  const { player: authPlayer } = useAuth()
  const [player, setPlayer] = useState<Player | null>(authPlayer)
  const [lastMatch, setLastMatch] = useState<Match | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    const load = async () => {
      // Prefer a fresh /me, but fall back to the player already in auth context
      // (a brand-new player may not have a stats record yet → /me 404s).
      const me = await client
        .get<Player>('/api/v1/players/me')
        .then(res => res.data)
        .catch(() => null)
      const matches = await client
        .get<Match[]>('/api/v1/matches', { params: { limit: 1 } })
        .then(res => res.data ?? [])
        .catch(() => [] as Match[])

      if (!active) return
      const resolved = me ?? authPlayer
      if (!resolved) {
        setError('Could not load your stats. Please try again.')
      } else {
        setPlayer(resolved)
        setLastMatch(matches[0] ?? null)
      }
    }
    load()
    return () => {
      active = false
    }
  }, [authPlayer])

  if (!player) {
    return (
      <PageContainer>
        {error ? (
          <ErrorBanner message={error} />
        ) : (
          <>
            <div className="mb-6 h-7 w-48 animate-pulse rounded-card bg-panel" />
            <SkeletonRows count={4} height="h-28" />
          </>
        )}
      </PageContainer>
    )
  }

  const { stats } = player
  const isEmpty = stats.totalMatches === 0
  const winRate =
    stats.totalMatches > 0
      ? Math.round((stats.wins / stats.totalMatches) * 100)
      : 0
  const won = lastMatch?.winnerTeam === 1

  if (isEmpty) {
    return (
      <PageContainer>
        <PageHeader
          eyebrow="/STATS"
          title="Your stats"
          subtitle="You haven't played a match yet."
        />

        <div className="mb-6 grid grid-cols-2 gap-5 lg:grid-cols-4">
          {['Total Score', 'Matches Played', 'Wins · Losses', 'Win Rate'].map(
            label => (
              <StatCard
                key={label}
                label={label}
                value={<span className="text-dim">—</span>}
                delta="no data yet"
              />
            ),
          )}
        </div>

        <Panel>
          <div className="flex flex-col items-center gap-5 px-6 py-20 text-center">
            <div className="flex flex-col gap-2">
              <p className="text-lg font-semibold text-fg">No matches yet</p>
              <p className="max-w-md text-[13px] leading-relaxed text-muted">
                Drop into your first battle and your scores, results and stats
                will land here automatically.
              </p>
            </div>
            <Button variant="primary" onClick={() => navigate('/play')}>
              Start your first game
            </Button>
          </div>
        </Panel>
      </PageContainer>
    )
  }

  return (
    <PageContainer>
      <PageHeader
        eyebrow="/STATS"
        title="Your stats"
        subtitle="Read-only report of your tank crew's campaign."
        action={
          <Button variant="primary" onClick={() => navigate('/play')}>
            Start game
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-2 gap-5 lg:grid-cols-4">
        <StatCard label="Total Score" value={stats.totalScore} />
        <StatCard label="Matches Played" value={stats.totalMatches} />
        <StatCard
          label="Wins · Losses"
          value={`${stats.wins} · ${stats.losses}`}
          tone="win"
        />
        <StatCard label="Win Rate" value={`${winRate}%`} tone="win" />
      </div>

      {lastMatch ? (
        <Panel header="Last match" meta={formatDateTime(lastMatch.createdAt)}>
          <div className="grid gap-6 p-[18px] lg:grid-cols-[1.4fr_1fr] lg:gap-8">
            <div className="flex items-center justify-center py-2">
              <Scoreboard
                ally={{
                  label: 'Ally · You',
                  score: lastMatch.playerScore,
                  sub: `captures · ${lastMatch.playerScore}`,
                }}
                enemy={{
                  label: 'Enemy · AI',
                  score: lastMatch.aiScore,
                  sub: `captures · ${lastMatch.aiScore}`,
                }}
              />
            </div>
            <div className="flex flex-col gap-4 border-t border-line pt-4 lg:border-t-0 lg:border-l lg:pt-1 lg:pl-8">
              <ResultBadge won={won} />
              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-1">
                  <Eyebrow>Duration</Eyebrow>
                  <span className="font-mono text-[22px] font-semibold text-fg">
                    {formatDuration(lastMatch.duration)}
                  </span>
                </div>
                <div className="flex flex-col gap-1">
                  <Eyebrow>Final score</Eyebrow>
                  <span className="font-mono text-[22px] font-semibold">
                    <span className="text-win">{lastMatch.playerScore}</span>
                    <span className="mx-1.5 text-dim">:</span>
                    <span className="text-loss">{lastMatch.aiScore}</span>
                  </span>
                </div>
              </div>
            </div>
          </div>
        </Panel>
      ) : (
        <Panel header="Last match">
          <div className="px-6 py-12 text-center font-mono text-sm text-muted">
            No recent match to show.
          </div>
        </Panel>
      )}
    </PageContainer>
  )
}

export default StatsPage
