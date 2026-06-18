import type { ReactNode } from 'react'

interface KeycapHintProps {
  keys: string
  children: ReactNode
}

/** A keycap chip followed by a label, e.g. [WASD] move. */
const KeycapHint = ({ keys, children }: KeycapHintProps) => (
  <span className="text-dim inline-flex items-center gap-2 font-mono text-[11px]">
    <kbd className="border-line bg-raised text-muted rounded-[3px] border px-2 py-1">
      {keys}
    </kbd>
    {children}
  </span>
)

export default KeycapHint
