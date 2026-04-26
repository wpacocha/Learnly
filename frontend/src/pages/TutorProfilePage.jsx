import { useEffect, useMemo, useState } from 'react'
import { createTutorProfile, getTutorProfile, updateTutorProfile } from '../api/tutorProfileApi'

const emptyProfile = {
  firstName: '',
  lastName: '',
  subject: '',
  teachingLevel: '',
  location: '',
  lessonMode: 'Hybrid',
  description: '',
  hourlyRate: '',
  photoUrl: '',
}

const toLegacyHeadline = (form) =>
  `${form.firstName} ${form.lastName} - ${form.subject}`.trim().slice(0, 200)

const toLegacyBio = (form) =>
  `Poziom: ${form.teachingLevel}\nTryb zajęć: ${form.lessonMode}\n${form.description || ''}`
    .trim()
    .slice(0, 4000)

const TutorProfilePage = () => {
  const [form, setForm] = useState(emptyProfile)
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [profileExists, setProfileExists] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    const loadProfile = async () => {
      setIsLoading(true)
      setError('')
      try {
        const profile = await getTutorProfile()
        if (!profile) {
          setProfileExists(false)
          setForm(emptyProfile)
          return
        }

        setForm({
          firstName: profile.firstName ?? '',
          lastName: profile.lastName ?? '',
          subject: profile.subject ?? '',
          teachingLevel: profile.teachingLevel ?? '',
          location: profile.location ?? '',
          lessonMode: profile.lessonMode ?? 'Hybrid',
          description: profile.description ?? '',
          hourlyRate: profile.hourlyRate?.toString() ?? '',
          photoUrl: profile.photoUrl ?? '',
        })
        setProfileExists(true)
      } catch (err) {
        setProfileExists(false)
        const status = err?.response?.status
        if (status === 401 || status === 403) {
          setError('To konto nie ma roli tutora. Zaloguj się jako Tutor.')
        } else {
          setError('Nie udało się pobrać profilu tutora.')
        }
      } finally {
        setIsLoading(false)
      }
    }

    loadProfile()
  }, [])

  const submitLabel = useMemo(
    () => (profileExists ? 'Zapisz zmiany' : 'Utwórz profil tutora'),
    [profileExists],
  )

  const handleChange = (event) => {
    const { name, value } = event.target
    setForm((prev) => {
      if (name === 'lessonMode' && value === 'Online') {
        return { ...prev, lessonMode: value, location: '' }
      }
      return { ...prev, [name]: value }
    })
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    setIsSubmitting(true)
    setError('')
    setMessage('')

    const payload = {
      firstName: form.firstName,
      lastName: form.lastName,
      subject: form.subject,
      teachingLevel: form.teachingLevel,
      location: form.lessonMode === 'Online' ? 'Online' : form.location,
      lessonMode: form.lessonMode,
      description: form.description,
      hourlyRate: Number(form.hourlyRate),
      photoUrl: form.photoUrl || null,
      // Backward compatibility for older backend payload contract.
      headline: toLegacyHeadline(form),
      bio: toLegacyBio(form),
    }

    try {
      if (profileExists) {
        await updateTutorProfile(payload)
        setMessage('Profil został zaktualizowany.')
      } else {
        await createTutorProfile(payload)
        setProfileExists(true)
        setMessage('Profil został utworzony.')
      }
    } catch (err) {
      const backendMessage = err?.response?.data?.message
      const validationErrors = err?.response?.data?.errors
      if (backendMessage) {
        setError(backendMessage)
      } else if (validationErrors && typeof validationErrors === 'object') {
        const firstError = Object.values(validationErrors)?.[0]?.[0]
        setError(firstError || 'Niepoprawne dane formularza.')
      } else if (err?.response?.status === 401 || err?.response?.status === 403) {
        setError('To konto nie ma uprawnień tutora. Zaloguj się jako Tutor.')
      } else if (err?.response?.status === 400) {
        setError('Niepoprawne dane formularza. Sprawdź wymagane pola.')
      } else {
        const fallbackBody = err?.response?.data
        if (fallbackBody) {
          setError(`Nie udało się zapisać profilu: ${JSON.stringify(fallbackBody)}`)
        } else {
          setError('Nie udało się zapisać profilu. Spróbuj ponownie.')
        }
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered">
          <p>Ładowanie profilu tutora...</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <section className="card">
        <h1>Profil tutora</h1>
        <p className="muted">Uzupełnij informacje, aby uczniowie mogli Cię łatwiej znaleźć.</p>
        <form className="form" onSubmit={handleSubmit}>
          <label htmlFor="firstName">Imię</label>
          <input
            id="firstName"
            name="firstName"
            value={form.firstName}
            onChange={handleChange}
            placeholder="np. Anna"
            required
          />

          <label htmlFor="lastName">Nazwisko</label>
          <input
            id="lastName"
            name="lastName"
            value={form.lastName}
            onChange={handleChange}
            placeholder="np. Kowalska"
            required
          />

          <label htmlFor="subject">Przedmiot</label>
          <input
            id="subject"
            name="subject"
            value={form.subject}
            onChange={handleChange}
            placeholder="np. Matematyka"
            required
          />

          <label htmlFor="teachingLevel">Poziom nauczania</label>
          <input
            id="teachingLevel"
            name="teachingLevel"
            value={form.teachingLevel}
            onChange={handleChange}
            placeholder="np. Matura, Liceum, A1, C1"
            required
          />

          <label htmlFor="lessonMode">Tryb zajęć</label>
          <select
            id="lessonMode"
            name="lessonMode"
            value={form.lessonMode}
            onChange={handleChange}
            required
          >
            <option value="Online">Online</option>
            <option value="Onsite">Stacjonarnie</option>
            <option value="Hybrid">Online i stacjonarnie</option>
          </select>

          {form.lessonMode !== 'Online' && (
            <>
              <label htmlFor="location">Lokalizacja</label>
              <input
                id="location"
                name="location"
                value={form.location}
                onChange={handleChange}
                placeholder="np. Warszawa"
                required
              />
            </>
          )}

          <label htmlFor="description">Opis (opcjonalny)</label>
          <textarea
            id="description"
            name="description"
            value={form.description}
            onChange={handleChange}
            rows={4}
            placeholder="Opisz swoje doświadczenie i styl nauczania."
          />

          <label htmlFor="hourlyRate">Stawka godzinowa (PLN)</label>
          <input
            id="hourlyRate"
            name="hourlyRate"
            type="number"
            min="1"
            value={form.hourlyRate}
            onChange={handleChange}
            required
          />

          <label htmlFor="photoUpload">Zdjęcie profilowe (opcjonalne)</label>
          <input
            id="photoUpload"
            accept="image/png,image/jpeg,image/webp"
            type="file"
            onChange={(event) => {
              const file = event.target.files?.[0]
              if (!file) {
                setForm((prev) => ({ ...prev, photoUrl: '' }))
                return
              }

              const reader = new FileReader()
              reader.onload = () => {
                const result = typeof reader.result === 'string' ? reader.result : ''
                setForm((prev) => ({ ...prev, photoUrl: result }))
              }
              reader.readAsDataURL(file)
            }}
          />
          {form.photoUrl && <p className="muted">Zdjęcie wybrane.</p>}

          {error && <p className="error">{error}</p>}
          {message && <p className="success">{message}</p>}

          <button className="btn primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Zapisywanie...' : submitLabel}
          </button>
        </form>
      </section>
    </main>
  )
}

export default TutorProfilePage
