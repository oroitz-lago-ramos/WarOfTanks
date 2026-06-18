interface ErrorBannerProps {
  message: string
}

const ErrorBanner = ({ message }: ErrorBannerProps) => (
  <div className="rounded-card border-loss/40 bg-loss/10 text-loss mb-6 border px-4 py-3 text-sm">
    {message}
  </div>
)

export default ErrorBanner
