import type { ReactNode } from 'react'

interface PanelProps {
  /** Optional mono uppercase header label (left side of the header strip). */
  header?: ReactNode
  /** Optional muted meta text (right side of the header strip). */
  meta?: ReactNode
  children: ReactNode
  className?: string
  /** Applied to the body wrapper — set padding here (tables stay flush by default). */
  bodyClassName?: string
}

/** Bordered surface with an optional header strip. */
const Panel = ({
  header,
  meta,
  children,
  className = '',
  bodyClassName = '',
}: PanelProps) => (
  <div
    className={`rounded-card border-line bg-panel overflow-hidden border ${className}`}
  >
    {(header || meta) && (
      <div className="border-line flex items-center justify-between gap-4 border-b px-[18px] py-[14px]">
        {header && (
          <span className="text-muted font-mono text-[11px] tracking-[1.32px] uppercase">
            {header}
          </span>
        )}
        {meta && <span className="text-dim font-mono text-[11px]">{meta}</span>}
      </div>
    )}
    <div className={bodyClassName}>{children}</div>
  </div>
)

export default Panel
