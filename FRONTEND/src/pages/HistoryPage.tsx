import { useEffect, useMemo, useState } from 'react'
import { client } from '../api/client'
import type { Match } from '../types'
import { formatDateTime, formatDuration } from '../utils/format'
import PageContainer from '../components/ui/PageContainer'
import PageHeader from '../components/ui/PageHeader'
import Panel from '../components/ui/Panel'
import Button from '../components/ui/Button'
import ResultBadge from '../components/ui/ResultBadge'
import DataTable, { type Column } from '../components/ui/DataTable'
import SegmentedFilter, { type SegmentOption } from '../components/ui/SegmentedFilter'
import SkeletonRows from '../components/ui/SkeletonRows'
import EmptyState from '../components/ui/EmptyState'
import ErrorBanner from '../components/ui/ErrorBanner'

const PAGE_SIZE = 10

type Filter = 'all' | 'win' | 'loss'

const FILTERS: SegmentOption<Filter>[] = [
  { value: 'all', label: 'All' },
  { value: 'win', label: 'Victories', tone: 'win' },
  { value: 'loss', label: 'Defeats', tone: 'loss' },
]

const isWin = (m: Match) => m.winnerTeam === 1

const columns: Column<Match>[] = [
  {
    key: 'date',
    header: 'Date',
    render: m => (
      <span className="font-mono text-[13px] text-muted">
        {formatDateTime(m.createdAt)}
      </span>
    ),
  },
  {
    key: 'score',
    header: 'Score',
    render: m => (
      <span className="font-mono text-sm">
        <span className="text-win">{m.playerScore}</span>
        <span className="mx-1.5 text-dim">:</span>
        <span className="text-loss">{m.aiScore}</span>
      </span>
    ),
  },
  {
    key: 'result',
    header: 'Result',
    render: m => <ResultBadge won={isWin(m)} />,
  },
  {
    key: 'duration',
    header: 'Duration',
    render: m => (
      <span className="font-mono text-sm text-muted">{formatDuration(m.duration)}</span>
    ),
  },
]

const HistoryPage = () => {
  const [matches, setMatches] = useState<Match[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [offset, setOffset] = useState(0)
  const [hasMore, setHasMore] = useState(true)
  const [filter, setFilter] = useState<Filter>('all')

  const fetchMatches = async (currentOffset: number, reset: boolean) => {
    setLoading(true)
    setError(null)
    try {
      const res = await client.get<Match[]>('/api/v1/matches', {
        params: { limit: PAGE_SIZE, offset: currentOffset },
      })
      const data = res.data ?? []
      setMatches(prev => (reset ? data : [...prev, ...data]))
      setHasMore(data.length === PAGE_SIZE)
      setOffset(currentOffset + data.length)
    } catch {
      setError('Could not load match history. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchMatches(0, true)
  }, [])

  const filtered = useMemo(() => {
    if (filter === 'win') return matches.filter(isWin)
    if (filter === 'loss') return matches.filter(m => !isWin(m))
    return matches
  }, [matches, filter])

  const avgDuration = filtered.length
    ? formatDuration(
        filtered.reduce((sum, m) => sum + m.duration, 0) / filtered.length,
      )
    : '—'

  const showSkeleton = loading && matches.length === 0

  return (
    <PageContainer>
      <PageHeader
        eyebrow="/MATCHES"
        title="Match history"
        subtitle="Your last battles, most recent first."
        action={
          <SegmentedFilter options={FILTERS} value={filter} onChange={setFilter} />
        }
      />

      {error && <ErrorBanner message={error} />}

      <Panel
        header={`${filtered.length} matches`}
        meta={`Avg duration · ${avgDuration}`}
      >
        {showSkeleton ? (
          <div className="p-4">
            <SkeletonRows count={6} />
          </div>
        ) : filtered.length === 0 ? (
          <EmptyState
            message={
              matches.length === 0
                ? 'No matches played yet.'
                : 'No matches for this filter.'
            }
          />
        ) : (
          <DataTable
            columns={columns}
            rows={filtered}
            rowKey={m => m.id}
            minWidth="min-w-[560px]"
          />
        )}
      </Panel>

      {!showSkeleton && hasMore && filter === 'all' && (
        <Button
          variant="outline"
          onClick={() => fetchMatches(offset, false)}
          disabled={loading}
          className="mt-4 w-full"
        >
          {loading ? 'Loading…' : 'Load more'}
        </Button>
      )}
    </PageContainer>
  )
}

export default HistoryPage
