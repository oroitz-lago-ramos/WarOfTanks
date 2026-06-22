import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { isAxiosError } from 'axios'
import { useNavigate } from 'react-router-dom'
import AuthHeading from '../components/auth/AuthHeading'
import AuthLayout from '../components/auth/AuthLayout'
import AuthMessage from '../components/auth/AuthMessage'
import AuthSubmitButton from '../components/auth/AuthSubmitButton'
import AuthSwitchLink from '../components/auth/AuthSwitchLink'
import AuthTextField from '../components/auth/AuthTextField'
import { client } from '../api/client'
import { useAuth } from '../hooks/useAuth'

interface RegisterErrors {
  username?: string
  email?: string
  password?: string
  confirmPassword?: string
  form?: string
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const USERNAME_PATTERN = /^[a-zA-Z0-9_]{3,20}$/

const RegisterPage = () => {
  const navigate = useNavigate()
  const { player } = useAuth()

  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [errors, setErrors] = useState<RegisterErrors>({})
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    if (player) {
      navigate('/leaderboard', { replace: true })
    }
  }, [navigate, player])

  const validate = () => {
    const nextErrors: RegisterErrors = {}
    const trimmedUsername = username.trim()
    const trimmedEmail = email.trim()

    if (!USERNAME_PATTERN.test(trimmedUsername)) {
      nextErrors.username = 'Use 3-20 letters, numbers, or underscores'
    }

    if (!EMAIL_PATTERN.test(trimmedEmail)) {
      nextErrors.email = 'Enter a valid email address'
    }

    if (password.length < 8) {
      nextErrors.password = 'Password must be at least 8 characters'
    }

    if (confirmPassword !== password) {
      nextErrors.confirmPassword = 'Passwords do not match'
    }

    return nextErrors
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const validationErrors = validate()
    setErrors(validationErrors)

    if (Object.keys(validationErrors).length > 0) {
      return
    }

    setIsSubmitting(true)

    try {
      await client.post('/api/v1/auth/register', {
        username: username.trim(),
        email: email.trim(),
        password,
      })

      navigate('/login', {
        replace: true,
        state: { message: 'Account created. You can sign in now.' },
      })
    } catch (error) {
      if (isAxiosError(error) && error.response?.status === 409) {
        setErrors({ username: 'Username already taken' })
        return
      }

      setErrors({ form: 'Could not create account. Try again.' })
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout coverImage="/CoverImage.png">
      <AuthHeading
        subtitle="One callsign, one tank crew."
        title="Create account"
      />

      {errors.form && (
        <AuthMessage className="mb-[18px]" variant="error">
          {errors.form}
        </AuthMessage>
      )}

      <form
        className="flex flex-col gap-[18px]"
        onSubmit={handleSubmit}
        noValidate
      >
        <AuthTextField
          autoComplete="username"
          error={errors.username}
          label="USERNAME - 3-20 CHARS"
          name="username"
          onChange={(event) => setUsername(event.target.value)}
          placeholder="callsign"
          type="text"
          value={username}
        />

        <AuthTextField
          autoComplete="email"
          error={errors.email}
          label="EMAIL"
          name="email"
          onChange={(event) => setEmail(event.target.value)}
          placeholder="email"
          type="email"
          value={email}
        />

        <AuthTextField
          autoComplete="new-password"
          error={errors.password}
          label="PASSWORD - MIN 8 CHARS"
          name="password"
          onChange={(event) => setPassword(event.target.value)}
          type="password"
          value={password}
        />

        <AuthTextField
          autoComplete="new-password"
          error={errors.confirmPassword}
          label="CONFIRM PASSWORD"
          name="confirmPassword"
          onChange={(event) => setConfirmPassword(event.target.value)}
          type="password"
          value={confirmPassword}
        />

        <AuthSubmitButton
          isLoading={isSubmitting}
          label="Create account"
          loadingLabel="Creating account"
        />
      </form>

      <AuthSwitchLink
        label="Already deployed?"
        linkLabel="Sign in"
        to="/login"
      />
    </AuthLayout>
  )
}

export default RegisterPage
