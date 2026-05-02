import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchSubjects, fetchTeachingLevels } from '../api/catalogApi'
import { resolveMediaUrl } from '../api/client'
import {
  createTutorProfile,
  getTutorProfile,
  updateTutorProfile,
  uploadTutorPhoto,
} from '../api/tutorProfileApi'
import { fetchMyAssignments, updateMyAssignments } from '../api/tutorAssignmentsApi'
import {
  createAvailabilitySlot,
  deleteAvailabilitySlot,
  fetchMyAvailabilitySlots,
} from '../api/tutorAvailabilityApi'

const emptyProfileForm = {
  firstName: '',
  lastName: '',
  bio: '',
}

const TEACHING_MODES = [
  { value: 0, label: 'Online' },
  { value: 1, label: 'Stacjonarnie' },
  { value: 2, label: 'Online i stacjonarnie' },
]

const DURATION_PRESETS = [
  { minutes: 30, label: '30 min' },
  { minutes: 45, label: '45 min' },
  { minutes: 60, label: '60 min' },
  { minutes: 90, label: '90 min' },
  { minutes: 120, label: '120 min' },
]

const newOfferingRow = () => ({
  clientKey: crypto.randomUUID(),
  serverId: null,
  subjectId: '',
  teachingMode: 0,
  location: '',
  hourlyRate: '',
  durationMinutes: 60,
  teachingLevelIds: [],
})

const formatWhen = (iso) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const TutorProfilePage = () => {
  const [form, setForm] = useState(emptyProfileForm)
  const [offerings, setOfferings] = useState([])
  /** false = zwinięte podsumowanie zapisanych pozycji; true = pełny edytor */
  const [offeringsUiExpanded, setOfferingsUiExpanded] = useState(true)
  const [subjects, setSubjects] = useState([])
  const [levels, setLevels] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSavingOfferings, setIsSavingOfferings] = useState(false)
  const [profileExists, setProfileExists] = useState(false)
  const [isEditingProfile, setIsEditingProfile] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [catalogError, setCatalogError] = useState('')
  const [availabilityModalForOfferingId, setAvailabilityModalForOfferingId] = useState(null)
  const [availabilitySlots, setAvailabilitySlots] = useState([])
  const [availabilityDate, setAvailabilityDate] = useState('')
  const [availabilityTime, setAvailabilityTime] = useState('09:00')
  const [availabilityError, setAvailabilityError] = useState('')
  const [availabilityMessage, setAvailabilityMessage] = useState('')
  const [isAvailabilityLoading, setIsAvailabilityLoading] = useState(false)
  const [isAvailabilitySaving, setIsAvailabilitySaving] = useState(false)
  const [availabilityDeletingId, setAvailabilityDeletingId] = useState(null)

  const [serverPhotoPath, setServerPhotoPath] = useState(null)
  const [pendingPhotoFile, setPendingPhotoFile] = useState(null)
  const [photoPreviewUrl, setPhotoPreviewUrl] = useState(null)
  const blobUrlRef = useRef(null)

  const revokeBlob = () => {
    if (blobUrlRef.current) {
      URL.revokeObjectURL(blobUrlRef.current)
      blobUrlRef.current = null
    }
  }

  const toggleLevelForOffering = useCallback((clientKey, levelId) => {
    setOfferings((prev) =>
      prev.map((row) => {
        if (row.clientKey !== clientKey) {
          return row
        }
        const on = row.teachingLevelIds.includes(levelId)
        return {
          ...row,
          teachingLevelIds: on ? row.teachingLevelIds.filter((x) => x !== levelId) : [...row.teachingLevelIds, levelId],
        }
      }),
    )
  }, [])

  const updateOfferingField = useCallback((clientKey, field, value) => {
    setOfferings((prev) =>
      prev.map((row) => (row.clientKey === clientKey ? { ...row, [field]: value } : row)),
    )
  }, [])

  const addOffering = useCallback(() => {
    setOfferingsUiExpanded(true)
    setOfferings((prev) => [...prev, newOfferingRow()])
  }, [])

  const removeOffering = useCallback((clientKey) => {
    setOfferings((prev) => prev.filter((r) => r.clientKey !== clientKey))
  }, [])

  useEffect(() => {
    const load = async () => {
      setIsLoading(true)
      setError('')
      setCatalogError('')

      try {
        const [s, l] = await Promise.all([fetchSubjects(), fetchTeachingLevels()])
        setSubjects(Array.isArray(s) ? s : [])
        setLevels(Array.isArray(l) ? l : [])
      } catch {
        setCatalogError(
          'Nie udało się pobrać katalogu przedmiotów i poziomów. Sprawdź połączenie z API i czy baza ma dane startowe.',
        )
        setSubjects([])
        setLevels([])
      }

      try {
        const profile = await getTutorProfile()
        if (!profile) {
          setProfileExists(false)
          setIsEditingProfile(false)
          setForm(emptyProfileForm)
          setOfferings([])
          setOfferingsUiExpanded(true)
          setServerPhotoPath(null)
          setPendingPhotoFile(null)
          revokeBlob()
          setPhotoPreviewUrl(null)
          return
        }

        setProfileExists(true)
        setIsEditingProfile(false)
        setForm({
          firstName: profile.firstName ?? '',
          lastName: profile.lastName ?? '',
          bio: profile.bio ?? '',
        })
        const path = profile.photoUrl ?? null
        setServerPhotoPath(path)
        setPendingPhotoFile(null)
        revokeBlob()
        setPhotoPreviewUrl(path ? resolveMediaUrl(path) : null)

        try {
          const list = await fetchMyAssignments()
          if (Array.isArray(list) && list.length > 0) {
            setOfferings(
              list.map((o) => ({
                clientKey: crypto.randomUUID(),
                serverId: o.id,
                subjectId: o.subjectId,
                teachingMode: o.teachingMode ?? 0,
                location: o.location ?? '',
                hourlyRate: o.hourlyRate != null ? String(o.hourlyRate) : '',
                durationMinutes: o.durationMinutes ?? 60,
                teachingLevelIds: Array.isArray(o.teachingLevelIds) ? [...o.teachingLevelIds] : [],
              })),
            )
            setOfferingsUiExpanded(false)
          } else {
            setOfferings([])
            setOfferingsUiExpanded(true)
          }
        } catch {
          setOfferings([])
          setOfferingsUiExpanded(true)
        }
      } catch (err) {
        setProfileExists(false)
        setIsEditingProfile(false)
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

    load()

    return () => {
      revokeBlob()
    }
  }, [])

  const submitLabel = useMemo(
    () => (profileExists ? 'Zapisz zmiany' : 'Utwórz profil tutora'),
    [profileExists],
  )

  const displayName = useMemo(() => {
    const a = form.firstName.trim()
    const b = form.lastName.trim()
    if (!a && !b) {
      return '—'
    }
    return [a, b].filter(Boolean).join(' ')
  }, [form.firstName, form.lastName])

  const handleCancelProfileEdit = async () => {
    setError('')
    setMessage('')
    try {
      const profile = await getTutorProfile()
      if (profile) {
        setForm({
          firstName: profile.firstName ?? '',
          lastName: profile.lastName ?? '',
          bio: profile.bio ?? '',
        })
        const path = profile.photoUrl ?? null
        setServerPhotoPath(path)
        setPendingPhotoFile(null)
        revokeBlob()
        setPhotoPreviewUrl(path ? resolveMediaUrl(path) : null)
        const input = document.getElementById('tutor-photo-input')
        if (input) {
          input.value = ''
        }
      }
    } catch {
      setError('Nie udało się przywrócić danych profilu.')
      return
    }
    setIsEditingProfile(false)
  }

  const handleChange = (event) => {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  const handlePhotoChange = (event) => {
    const file = event.target.files?.[0]
    revokeBlob()
    if (!file) {
      setPendingPhotoFile(null)
      setPhotoPreviewUrl(serverPhotoPath ? resolveMediaUrl(serverPhotoPath) : null)
      return
    }

    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      setError('Wybierz plik JPEG, PNG lub WebP.')
      event.target.value = ''
      return
    }

    if (file.size > 5 * 1024 * 1024) {
      setError('Maksymalny rozmiar zdjęcia to 5 MB.')
      event.target.value = ''
      return
    }

    setError('')
    setPendingPhotoFile(file)
    const blobUrl = URL.createObjectURL(file)
    blobUrlRef.current = blobUrl
    setPhotoPreviewUrl(blobUrl)
  }

  const clearPhoto = () => {
    revokeBlob()
    setPendingPhotoFile(null)
    setServerPhotoPath(null)
    setPhotoPreviewUrl(null)
    const input = document.getElementById('tutor-photo-input')
    if (input) {
      input.value = ''
    }
  }

  const handleSubmitProfile = async (event) => {
    event.preventDefault()
    setIsSubmitting(true)
    setError('')
    setMessage('')

    if (!form.firstName.trim() || !form.lastName.trim()) {
      setError('Podaj imię i nazwisko.')
      setIsSubmitting(false)
      return
    }

    if (!form.bio.trim()) {
      setError('Podaj krótki opis (bio).')
      setIsSubmitting(false)
      return
    }

    let photoUrlToSave = null
    if (pendingPhotoFile) {
      try {
        photoUrlToSave = await uploadTutorPhoto(pendingPhotoFile)
      } catch (err) {
        const d = err?.response?.data
        setError(d?.message || d?.detail || 'Nie udało się przesłać zdjęcia.')
        setIsSubmitting(false)
        return
      }
    } else if (serverPhotoPath && photoPreviewUrl) {
      photoUrlToSave = serverPhotoPath
    }

    if (photoUrlToSave && photoUrlToSave.length > 2000) {
      setError('Zapisana ścieżka zdjęcia jest zbyt długa — zgłoś to administratorowi.')
      setIsSubmitting(false)
      return
    }

    const payload = {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      bio: form.bio.trim(),
      photoUrl: photoUrlToSave,
    }

    try {
      let saved
      if (profileExists) {
        saved = await updateTutorProfile(payload)
        setMessage('Profil został zaktualizowany.')
        setIsEditingProfile(false)
      } else {
        saved = await createTutorProfile(payload)
        setProfileExists(true)
        setIsEditingProfile(false)
        setMessage('Profil został utworzony. Dodaj poniżej przedmioty, tryb i stawki dla każdej pozycji oferty.')
      }

      const newPath = saved?.photoUrl ?? photoUrlToSave
      setServerPhotoPath(newPath ?? null)
      revokeBlob()
      setPendingPhotoFile(null)
      setPhotoPreviewUrl(newPath ? resolveMediaUrl(newPath) : null)
      const input = document.getElementById('tutor-photo-input')
      if (input) {
        input.value = ''
      }
    } catch (err) {
      const d = err?.response?.data
      const backendMessage = d?.message
      const detail = d?.detail
      const title = d?.title
      const validationErrors = d?.errors
      if (backendMessage) {
        setError(backendMessage)
      } else if (detail) {
        setError(typeof detail === 'string' ? detail : JSON.stringify(detail))
      } else if (title) {
        setError(`${title}${detail ? `: ${detail}` : ''}`)
      } else if (validationErrors && typeof validationErrors === 'object') {
        const firstError = Object.values(validationErrors)?.[0]?.[0]
        setError(firstError || 'Niepoprawne dane formularza.')
      } else if (err?.response?.status === 409) {
        setError('Profil już istnieje — odśwież stronę lub użyj „Zapisz profil”.')
        setProfileExists(true)
      } else {
        setError(
          typeof d === 'string'
            ? d
            : 'Nie udało się zapisać profilu. Upewnij się, że baza ma aktualny schemat (migracje EF).',
        )
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleSaveOfferings = async (event) => {
    event.preventDefault()
    if (!profileExists) {
      setError('Najpierw utwórz profil powyżej.')
      return
    }
    if (offerings.length === 0) {
      setError('Dodaj co najmniej jedną pozycję oferty (przycisk +).')
      return
    }

    const body = { offerings: [] }
    for (const row of offerings) {
      if (!row.subjectId) {
        setError('W każdej pozycji wybierz przedmiot.')
        return
      }
      const rate = Number(row.hourlyRate)
      if (!Number.isFinite(rate) || rate < 0.01) {
        setError('Podaj poprawną stawkę (min. 0,01 PLN) dla każdej pozycji.')
        return
      }
      const dm = Number(row.durationMinutes)
      if (!Number.isFinite(dm) || dm < 15 || dm > 480) {
        setError('Długość lekcji musi być między 15 a 480 minut.')
        return
      }
      if (row.teachingMode === 1 || row.teachingMode === 2) {
        if (!String(row.location || '').trim()) {
          setError('Dla trybu stacjonarnego lub hybrydowego podaj lokalizację.')
          return
        }
      }
      if (!row.teachingLevelIds?.length) {
        setError('Każda pozycja musi mieć co najmniej jeden poziom nauczania.')
        return
      }

      const item = {
        subjectId: Number(row.subjectId),
        teachingMode: row.teachingMode,
        location:
          row.teachingMode === 1 || row.teachingMode === 2 ? String(row.location).trim() : null,
        hourlyRate: rate,
        durationMinutes: dm,
        teachingLevelIds: row.teachingLevelIds.map(Number),
      }
      if (row.serverId) {
        item.id = row.serverId
      }
      body.offerings.push(item)
    }

    setIsSavingOfferings(true)
    setError('')
    setMessage('')
    try {
      const saved = await updateMyAssignments(body)
      if (Array.isArray(saved)) {
        setOfferings(
          saved.map((o) => ({
            clientKey: crypto.randomUUID(),
            serverId: o.id,
            subjectId: o.subjectId,
            teachingMode: o.teachingMode ?? 0,
            location: o.location ?? '',
            hourlyRate: o.hourlyRate != null ? String(o.hourlyRate) : '',
            durationMinutes: o.durationMinutes ?? 60,
            teachingLevelIds: Array.isArray(o.teachingLevelIds) ? [...o.teachingLevelIds] : [],
          })),
        )
      }
      setMessage('Oferta (przedmioty i poziomy) została zapisana.')
      setOfferingsUiExpanded(false)
    } catch (err) {
      const backendMessage = err?.response?.data?.message
      setError(backendMessage || 'Nie udało się zapisać oferty.')
    } finally {
      setIsSavingOfferings(false)
    }
  }

  const subjectById = useMemo(() => new Map(subjects.map((s) => [s.id, s.name])), [subjects])
  const levelById = useMemo(() => new Map(levels.map((l) => [l.id, l.name])), [levels])

  const teachingModeLabel = (m) => TEACHING_MODES.find((x) => x.value === m)?.label ?? '—'

  const formatPln = (v) =>
    Number(v).toLocaleString('pl-PL', { minimumFractionDigits: 0, maximumFractionDigits: 2 })

  const canCollapseOfferings =
    offeringsUiExpanded &&
    offerings.length > 0 &&
    offerings.every((o) => o.serverId != null && String(o.serverId).length > 0)

  const showOfferingsEditor = offeringsUiExpanded || offerings.length === 0
  const availabilityOffering = useMemo(
    () => offerings.find((o) => o.serverId === availabilityModalForOfferingId) ?? null,
    [offerings, availabilityModalForOfferingId],
  )
  const availabilitySlotsForSelected = useMemo(
    () =>
      availabilityModalForOfferingId
        ? availabilitySlots
            .filter((s) => s.tutorTeachingOfferingId === availabilityModalForOfferingId)
            .sort((a, b) => new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime())
        : [],
    [availabilitySlots, availabilityModalForOfferingId],
  )

  const reloadAvailabilitySlots = useCallback(async () => {
    const data = await fetchMyAvailabilitySlots()
    setAvailabilitySlots(Array.isArray(data) ? data : [])
  }, [])

  const openAvailabilityForOffering = async (row) => {
    if (!row.serverId) {
      return
    }
    setAvailabilityModalForOfferingId(row.serverId)
    setAvailabilityError('')
    setAvailabilityMessage('')
    setAvailabilityDate('')
    setAvailabilityTime('09:00')
    setIsAvailabilityLoading(true)
    try {
      await reloadAvailabilitySlots()
    } catch {
      setAvailabilityError('Nie udało się pobrać dostępności.')
    } finally {
      setIsAvailabilityLoading(false)
    }
  }

  const closeAvailabilityModal = () => {
    setAvailabilityModalForOfferingId(null)
    setAvailabilityDate('')
    setAvailabilityTime('09:00')
    setAvailabilityError('')
    setAvailabilityMessage('')
  }

  const handleAddAvailabilityFromModal = async (event) => {
    event.preventDefault()
    if (!availabilityModalForOfferingId) {
      return
    }
    if (!availabilityDate) {
      setAvailabilityError('Wybierz datę.')
      return
    }
    if (!availabilityTime) {
      setAvailabilityError('Wybierz godzinę.')
      return
    }
    const start = new Date(`${availabilityDate}T${availabilityTime}`)
    if (Number.isNaN(start.getTime())) {
      setAvailabilityError('Niepoprawna data początku.')
      return
    }
    setIsAvailabilitySaving(true)
    setAvailabilityError('')
    setAvailabilityMessage('')
    try {
      await createAvailabilitySlot({
        tutorTeachingOfferingId: availabilityModalForOfferingId,
        startUtc: start.toISOString(),
      })
      setAvailabilityDate('')
      setAvailabilityTime('09:00')
      setAvailabilityMessage('Dodano termin.')
      try {
        await reloadAvailabilitySlots()
      } catch {
        // Slot jest już zapisany; w tym kroku tylko odświeżamy listę.
      }
    } catch (err) {
      setAvailabilityError(err?.response?.data?.message || 'Nie udało się dodać terminu.')
    } finally {
      setIsAvailabilitySaving(false)
    }
  }

  const handleDeleteAvailabilityFromModal = async (slotId) => {
    setAvailabilityDeletingId(slotId)
    setAvailabilityError('')
    try {
      await deleteAvailabilitySlot(slotId)
      await reloadAvailabilitySlots()
    } catch {
      setAvailabilityError('Usuwanie terminu nie powiodło się.')
    } finally {
      setAvailabilityDeletingId(null)
    }
  }

  const expandOfferingsAndScroll = (clientKey) => {
    setOfferingsUiExpanded(true)
    setMessage('')
    setError('')
    window.requestAnimationFrame(() => {
      document.getElementById(`offering-edit-${clientKey}`)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
    })
  }

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered loading-state">
          <p>Ładowanie profilu tutora…</p>
        </section>
      </main>
    )
  }

  const catalogReady = subjects.length > 0 && levels.length > 0

  return (
    <main className="page-shell tutor-profile-page">
      {(error || message || catalogError) && (
        <div className="alert-stack" aria-live="polite">
          {catalogError && (
            <div className="alert alert-warning" role="status">
              {catalogError}
            </div>
          )}
          {error && (
            <div className="alert alert-error" role="alert">
              {error}
            </div>
          )}
          {message && (
            <div className="alert alert-success" role="status">
              {message}
            </div>
          )}
        </div>
      )}

      <section className="card">
        <h1>Profil tutora</h1>

        {profileExists && !isEditingProfile ? (
          <>
            <p className="muted">
              Twoje dane są zapisane. Poniżej ustawiasz ofertę: przedmioty, tryb, stawkę i długość lekcji dla każdej
              pozycji.
            </p>
            <div className="tutor-profile-summary">
              {photoPreviewUrl && (
                <img className="tutor-profile-summary-photo" src={photoPreviewUrl} alt="" />
              )}
              <div className="tutor-profile-summary-body">
                <h2 className="tutor-profile-summary-headline">{displayName}</h2>
                {form.bio.trim() && <p className="tutor-profile-summary-bio">{form.bio.trim()}</p>}
              </div>
            </div>
            <button
              type="button"
              className="btn primary"
              onClick={() => {
                setError('')
                setMessage('')
                setIsEditingProfile(true)
              }}
            >
              Edytuj profil
            </button>
          </>
        ) : (
          <>
            <p className="muted">
              {profileExists
                ? 'Zaktualizuj imię, nazwisko i opis. Zdjęcie jest opcjonalne.'
                : 'Zapisz imię, nazwisko i opis. Zdjęcie opcjonalnie. Potem zdefiniuj ofertę (przedmioty + tryby) w drugiej sekcji.'}
            </p>
            <form className="form" onSubmit={handleSubmitProfile} noValidate>
              <label htmlFor="firstName">Imię</label>
              <input
                id="firstName"
                name="firstName"
                value={form.firstName}
                onChange={handleChange}
                required
                maxLength={100}
                autoComplete="given-name"
              />

              <label htmlFor="lastName">Nazwisko</label>
              <input
                id="lastName"
                name="lastName"
                value={form.lastName}
                onChange={handleChange}
                required
                maxLength={100}
                autoComplete="family-name"
              />

              <label htmlFor="bio">Opis (bio)</label>
              <textarea
                id="bio"
                name="bio"
                value={form.bio}
                onChange={handleChange}
                rows={5}
                placeholder="Doświadczenie, styl pracy, czego uczysz."
                required
                maxLength={4000}
              />

              <div className="photo-upload-block">
                <label htmlFor="tutor-photo-input">Zdjęcie profilowe — opcjonalne</label>
                <p className="muted small-print">
                  Możesz pominąć to pole i od razu zapisać profil. Jeśli dodasz zdjęcie: JPEG, PNG lub WebP, do 5 MB.
                </p>
                <input
                  id="tutor-photo-input"
                  accept="image/jpeg,image/png,image/webp"
                  type="file"
                  onChange={handlePhotoChange}
                />
                {(photoPreviewUrl || serverPhotoPath) && (
                  <div className="photo-preview-row">
                    {photoPreviewUrl && (
                      <img className="tutor-photo-preview" src={photoPreviewUrl} alt="Podgląd zdjęcia profilowego" />
                    )}
                    <button type="button" className="btn ghost" onClick={clearPhoto}>
                      Usuń zdjęcie
                    </button>
                  </div>
                )}
              </div>

              <div className="form-actions-row">
                <button className="btn primary" type="submit" disabled={isSubmitting}>
                  {isSubmitting ? 'Zapisywanie...' : submitLabel}
                </button>
                {profileExists && (
                  <button
                    type="button"
                    className="btn ghost"
                    disabled={isSubmitting}
                    onClick={handleCancelProfileEdit}
                  >
                    Anuluj
                  </button>
                )}
              </div>
            </form>
          </>
        )}
      </section>

      <section className={`card tutor-assignments-card ${!catalogReady ? 'card-disabled' : ''}`}>
        <h2>Przedmioty, tryb i poziomy</h2>
        <p className="muted">
          {showOfferingsEditor
            ? 'Dodaj pozycje oferty przyciskiem „+”. Dla każdej ustaw tryb, stawkę, długość lekcji i poziomy. Po zapisie lista zwinie się do podglądu — stamtąd ustawisz godziny lub wrócisz do edycji.'
            : 'Twoja zapisana oferta — edytuj szczegóły albo przejdź do ustalania dostępnych terminów dla wybranej pozycji.'}{' '}
          <Link className="inline-link" to="/tutor/availability">
            Wszystkie terminy →
          </Link>
        </p>

        {!catalogReady && (
          <div className="empty-catalog-hint" role="status">
            <p>
              <strong>Brak pozycji w katalogu.</strong> Uruchom ponownie backend — przy starcie do pustej bazy dodawane są
              przedmioty i poziomy. Jeśli to nowa baza, wykonaj też migracje EF.
            </p>
          </div>
        )}

        {!showOfferingsEditor && offerings.length > 0 && (
          <div className="tutor-offerings-summary-block">
            <ul className="tutor-offerings-summary-list" aria-label="Zapisana oferta">
              {offerings.map((row) => {
                const subj = subjectById.get(row.subjectId) ?? '—'
                const levelNames = row.teachingLevelIds
                  .map((id) => levelById.get(id))
                  .filter(Boolean)
                  .join(', ')
                return (
                  <li key={row.clientKey} className="tutor-offering-summary">
                    <div className="tutor-offering-summary__main">
                      <div className="tutor-offering-summary__title">{subj}</div>
                      <div className="tutor-offering-summary__meta">
                        {teachingModeLabel(row.teachingMode)}
                        {(row.teachingMode === 1 || row.teachingMode === 2) && row.location?.trim()
                          ? ` · ${row.location.trim()}`
                          : ''}
                        {' · '}
                        {formatPln(row.hourlyRate || 0)} PLN/h · {row.durationMinutes} min
                      </div>
                      {levelNames && <div className="tutor-offering-summary__levels muted">Poziomy: {levelNames}</div>}
                    </div>
                    <div className="tutor-offering-summary__actions">
                      <button
                        type="button"
                        className="btn secondary"
                        onClick={() => expandOfferingsAndScroll(row.clientKey)}
                        disabled={!catalogReady}
                      >
                        Edytuj
                      </button>
                      <button
                        type="button"
                        className="btn primary"
                        onClick={() => openAvailabilityForOffering(row)}
                        disabled={!catalogReady || !row.serverId}
                      >
                        Ustaw godziny
                      </button>
                    </div>
                  </li>
                )
              })}
            </ul>
            <div className="tutor-offerings-summary-toolbar">
              <button
                type="button"
                className="btn ghost"
                onClick={() => {
                  setOfferingsUiExpanded(true)
                  setMessage('')
                  setError('')
                }}
                disabled={!catalogReady}
              >
                Edytuj całą ofertę
              </button>
              <button type="button" className="btn secondary" onClick={addOffering} disabled={!catalogReady}>
                + Dodaj pozycję oferty
              </button>
            </div>
          </div>
        )}

        {showOfferingsEditor && (
          <>
            <div className="tutor-offerings-actions">
              <button type="button" className="btn secondary" onClick={addOffering} disabled={!catalogReady}>
                + Dodaj pozycję oferty
              </button>
              {canCollapseOfferings && (
                <button
                  type="button"
                  className="btn ghost"
                  onClick={() => {
                    setOfferingsUiExpanded(false)
                    setMessage('')
                    setError('')
                  }}
                >
                  Zwiń podgląd
                </button>
              )}
            </div>

            {offerings.length === 0 && catalogReady && (
              <p className="muted">Brak pozycji — kliknij „+”, aby dodać pierwszy przedmiot.</p>
            )}

            <div className="tutor-offerings-list">
              {offerings.map((row) => (
                <div key={row.clientKey} id={`offering-edit-${row.clientKey}`} className="card tutor-offering-card">
                  <div className="tutor-offering-card__head">
                    <span className="tutor-offering-card__title">Pozycja oferty</span>
                    <button
                      type="button"
                      className="btn ghost"
                      onClick={() => removeOffering(row.clientKey)}
                      disabled={!catalogReady}
                    >
                      Usuń
                    </button>
                  </div>

                  <div className="form tutor-offering-fields">
                <label>Przedmiot</label>
                <select
                  value={row.subjectId === '' ? '' : String(row.subjectId)}
                  onChange={(e) =>
                    updateOfferingField(row.clientKey, 'subjectId', e.target.value ? Number(e.target.value) : '')
                  }
                  disabled={!catalogReady}
                >
                  <option value="">— wybierz —</option>
                  {subjects.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
                </select>

                <label>Tryb</label>
                <select
                  value={row.teachingMode}
                  onChange={(e) => updateOfferingField(row.clientKey, 'teachingMode', Number(e.target.value))}
                  disabled={!catalogReady}
                >
                  {TEACHING_MODES.map((m) => (
                    <option key={m.value} value={m.value}>
                      {m.label}
                    </option>
                  ))}
                </select>

                {(row.teachingMode === 1 || row.teachingMode === 2) && (
                  <>
                    <label>Lokalizacja (stacjonarnie)</label>
                    <input
                      type="text"
                      value={row.location}
                      onChange={(e) => updateOfferingField(row.clientKey, 'location', e.target.value)}
                      maxLength={300}
                      placeholder="np. Warszawa, Śródmieście"
                      disabled={!catalogReady}
                    />
                  </>
                )}

                <label>Stawka godzinowa (PLN)</label>
                <input
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={row.hourlyRate}
                  onChange={(e) => updateOfferingField(row.clientKey, 'hourlyRate', e.target.value)}
                  disabled={!catalogReady}
                />

                <label>Długość lekcji</label>
                <select
                  value={row.durationMinutes}
                  onChange={(e) => updateOfferingField(row.clientKey, 'durationMinutes', Number(e.target.value))}
                  disabled={!catalogReady}
                >
                  {DURATION_PRESETS.map((d) => (
                    <option key={d.minutes} value={d.minutes}>
                      {d.label}
                    </option>
                  ))}
                </select>

                <fieldset className="checkbox-fieldset levels-chip-fieldset" disabled={!catalogReady}>
                  <legend>Poziomy nauczania</legend>
                  <div className="chip-select-grid" role="group">
                    {levels.map((l) => {
                      const on = row.teachingLevelIds.includes(l.id)
                      return (
                        <button
                          key={l.id}
                          type="button"
                          className={`chip-toggle${on ? ' chip-toggle--on' : ''}`}
                          aria-pressed={on}
                          onClick={() => toggleLevelForOffering(row.clientKey, l.id)}
                        >
                          {l.name}
                        </button>
                      )
                    })}
                  </div>
                </fieldset>
                  </div>
                </div>
              ))}
            </div>

            <form className="tutor-offerings-save-form" onSubmit={handleSaveOfferings}>
              <button
                className="btn secondary"
                type="submit"
                disabled={isSavingOfferings || !profileExists || !catalogReady}
              >
                {isSavingOfferings ? 'Zapisywanie...' : 'Zapisz ofertę (przedmioty i poziomy)'}
              </button>
            </form>
          </>
        )}
      </section>

      {availabilityModalForOfferingId && (
        <div className="availability-modal-backdrop" role="presentation" onClick={closeAvailabilityModal}>
          <section
            className="availability-modal card"
            role="dialog"
            aria-modal="true"
            aria-label="Ustal dostępne godziny"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="availability-modal__head">
              <h2>Ustal dostępne godziny</h2>
              <button type="button" className="btn ghost" onClick={closeAvailabilityModal}>
                Zamknij
              </button>
            </div>
            <p className="muted">
              {availabilityOffering
                ? `${subjectById.get(availabilityOffering.subjectId) ?? 'Przedmiot'} · ${teachingModeLabel(availabilityOffering.teachingMode)} · ${availabilityOffering.durationMinutes} min`
                : 'Pozycja oferty'}
            </p>

            {availabilityError && (
              <div className="alert alert-error" role="alert">
                {availabilityError}
              </div>
            )}
            {availabilityMessage && (
              <div className="alert alert-success" role="status">
                {availabilityMessage}
              </div>
            )}

            <form className="form availability-modal__form" onSubmit={handleAddAvailabilityFromModal}>
              <label htmlFor="availabilityModalStart">Początek slotu</label>
              <div className="availability-modal__row">
                <input
                  id="availabilityModalDate"
                  type="date"
                  value={availabilityDate}
                  onChange={(e) => setAvailabilityDate(e.target.value)}
                  required
                />
                <input
                  id="availabilityModalTime"
                  type="time"
                  value={availabilityTime}
                  step={300}
                  onChange={(e) => setAvailabilityTime(e.target.value)}
                  required
                />
              </div>
              <button className="btn primary" type="submit" disabled={isAvailabilitySaving}>
                {isAvailabilitySaving ? 'Dodawanie...' : 'Dodaj slot'}
              </button>
            </form>

            <div className="availability-modal__list">
              <h3>Terminy dla tej pozycji</h3>
              {isAvailabilityLoading ? (
                <p className="muted">Ładowanie terminów...</p>
              ) : availabilitySlotsForSelected.length === 0 ? (
                <p className="muted">Brak zdefiniowanych terminów.</p>
              ) : (
                <ul className="slot-list">
                  {availabilitySlotsForSelected.map((slot) => (
                    <li key={slot.id} className="slot-row">
                      <span>
                        {formatWhen(slot.startUtc)} — {formatWhen(slot.endUtc)}
                      </span>
                      <button
                        type="button"
                        className="btn secondary"
                        disabled={availabilityDeletingId === slot.id}
                        onClick={() => handleDeleteAvailabilityFromModal(slot.id)}
                      >
                        {availabilityDeletingId === slot.id ? 'Usuwanie...' : 'Usuń'}
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </section>
        </div>
      )}
    </main>
  )
}

export default TutorProfilePage
