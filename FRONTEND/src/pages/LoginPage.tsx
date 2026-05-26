import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import AuthHeading from '../components/auth/AuthHeading'
import AuthLayout from '../components/auth/AuthLayout'
import AuthMessage from '../components/auth/AuthMessage'
import AuthSubmitButton from '../components/auth/AuthSubmitButton'
import AuthSwitchLink from '../components/auth/AuthSwitchLink'
import AuthTextField from '../components/auth/AuthTextField'
import { useAuth } from '../hooks/useAuth'

interface LoginLocationState {
  message?: string
}

const LoginPage = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { player, login } = useAuth()
  const state = location.state as LoginLocationState | null

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    if (player) {
      navigate('/leaderboard', { replace: true })
    }
  }, [navigate, player])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      await login(username.trim(), password)
      navigate('/leaderboard', { replace: true })
    } catch {
      setError('Invalid username or password')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout>
      <AuthHeading
        subtitle="Pick up your tank crew where you left them."
        title="Sign in"
      />

      {state?.message && (
        <AuthMessage className="mb-[18px]" variant="success">
          {state.message}
        </AuthMessage>
      )}

      <form
        className="flex flex-col gap-[18px]"
        onSubmit={handleSubmit}
        noValidate
      >
        <AuthTextField
          autoComplete="username"
          hasError={Boolean(error)}
          label="USERNAME"
          name="username"
          onChange={(event) => setUsername(event.target.value)}
          type="text"
          value={username}
        />

        <AuthTextField
          autoComplete="current-password"
          hasError={Boolean(error)}
          label="PASSWORD"
          name="password"
          onChange={(event) => setPassword(event.target.value)}
          type="password"
          value={password}
        />

        {error && <AuthMessage variant="error">{error}</AuthMessage>}

        <AuthSubmitButton
          isLoading={isSubmitting}
          label="Login"
          loadingLabel="Logging in"
        />
      </form>

      <AuthSwitchLink
        label="No account yet?"
        linkLabel="Create one"
        to="/register"
      />
    </AuthLayout>
  )
}

export default LoginPage
