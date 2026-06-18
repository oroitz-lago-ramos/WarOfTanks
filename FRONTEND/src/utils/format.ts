const pad = (n: number) => String(n).padStart(2, '0')

/** "2026-05-12 21:14" */
export const formatDateTime = (iso: string) => {
  const d = new Date(iso)
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

/** "08:42" (mm:ss) */
export const formatDuration = (secs: number) => {
  const m = Math.floor(secs / 60)
  const s = Math.round(secs % 60)
  return `${pad(m)}:${pad(s)}`
}
