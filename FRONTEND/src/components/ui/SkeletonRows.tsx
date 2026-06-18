interface SkeletonRowsProps {
  count?: number
  height?: string
}

const SkeletonRows = ({ count = 6, height = 'h-14' }: SkeletonRowsProps) => (
  <div className="space-y-px">
    {Array.from({ length: count }).map((_, i) => (
      <div
        key={i}
        className={`${height} rounded-card bg-panel animate-pulse`}
      />
    ))}
  </div>
)

export default SkeletonRows
