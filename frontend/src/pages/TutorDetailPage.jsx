import { useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import { resolveMediaUrl } from '../api/client'
import { bookLesson } from '../api/lessonsApi'
import { fetchReviewsForTutor } from '../api/reviewsApi'
import { searchTutors } from '../api/tutorSearchApi'
import { fetchSubjects, fetchTeachingLevels } from '../api/catalogApi'
import { useAuth } from '../context/AuthContext'

const formatWhen = (iso) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const formatSubjectNames = (ids, byId) =>
  ids?.length ? ids.map((id) => byId.get(id) ?? `#${id}`).join(', ') : '—'

const formatPln = (value) =>
  Number(value).toLocaleString('pl-PL', { minimumFractionDigits: 0, maximumFractionDigits: 2 })

const toDateKey = (date) =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`

const startOfWeek = (date) => {
  const next = new Date(date)
  const day = (next.getDay() + 6) % 7
  next.setHours(0, 0, 0, 0)
  next.setDate(next.getDate() - day)
  return next
}

const addDays = (date, days) => {
  const next = new Date(date)
  next.setDate(next.getDate() + days)
  return next
}

const teachingModeLabel = (m) => {
  if (m === 0) return 'Online'
  if (m === 1) return 'Stacjonarnie'
  return 'Online i stacjonarnie'
}

const TutorDetailPage = () => {
  const { tutorProfileId } = useParams()
  const location = useLocation()
  const navigate = useNavigate()
  const { user } = useAuth()

  const seeded = location.state?.tutor
  const seededOk = Boolean(tutorProfileId && seeded?.tutorProfileId === tutorProfileId)
  const [tutor, setTutor] = useState(seededOk ? seeded : null)
  const [reviews, setReviews] = useState([])
  const [subjects, setSubjects] = useState([])
  const [levels, setLevels] = useState([])
  const [loadError, setLoadError] = useState('')
  const [bookingSlotId, setBookingSlotId] = useState(null)
  const [bookMessage, setBookMessage] = useState('')
  const [bookError, setBookError] = useState('')
  const [slotSubjectFilter, setSlotSubjectFilter] = useState('')
  const [weekStart, setWeekStart] = useState(() => startOfWeek(new Date()))

  const isStudent = user?.roles?.includes('Student')

  const subjectById = useMemo(() => new Map(subjects.map((s) => [s.id, s.name])), [subjects])
  const levelById = useMemo(() => new Map(levels.map((l) => [l.id, l.name])), [levels])

  const avgRating = useMemo(() => {
    if (!reviews.length) {
      return null
    }
    const sum = reviews.reduce((acc, r) => acc + r.rating, 0)
    return (sum / reviews.length).toFixed(1)
  }, [reviews])

  useEffect(() => {
    const loadCatalog = async () => {
      try {
        const [catalogS, catalogL] = await Promise.all([fetchSubjects(), fetchTeachingLevels()])
        setSubjects(catalogS)
        setLevels(catalogL)
      } catch {
        /* nazwy przedmiotów mogą zostać jako ID */
      }
    }
    loadCatalog()
  }, [])

  useEffect(() => {
    if (!tutorProfileId) {
      return undefined
    }

    let cancelled = false
    ;(async () => {
      setLoadError('')
      try {
        const data = await searchTutors({})
        if (cancelled) {
          return
        }
        const found = data.find((t) => t.tutorProfileId === tutorProfileId)
        if (!found) {
          setTutor(null)
          setLoadError('Nie znaleziono tutora. Wróć do wyszukiwania.')
          return
        }
        setTutor(found)
      } catch {
        if (!cancelled) {
          setTutor(null)
          setLoadError('Nie udało się załadować profilu tutora.')
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [tutorProfileId])

  useEffect(() => {
    const loadReviews = async () => {
      if (!tutorProfileId) {
        return
      }
      try {
        const data = await fetchReviewsForTutor(tutorProfileId)
        setReviews(Array.isArray(data) ? data : [])
      } catch {
        setReviews([])
      }
    }
    loadReviews()
  }, [tutorProfileId])

  const futureSlots = (tutor?.availableSlots ?? []).filter((s) => new Date(s.startUtc) > new Date())
  const filteredSlots = futureSlots.filter((slot) =>
    slotSubjectFilter ? String(slot.subjectId) === String(slotSubjectFilter) : true,
  )
  const weekDays = Array.from({ length: 7 }, (_, index) => addDays(weekStart, index))
  const slotsByDay = new Map()
  filteredSlots.forEach((slot) => {
    const key = toDateKey(new Date(slot.startUtc))
    slotsByDay.set(key, [...(slotsByDay.get(key) ?? []), slot])
  })

  const handleBook = async (slotId) => {
    if (!isStudent) {
      setBookError('Tylko uczeń może rezerwować lekcje.')
      return
    }
    setBookingSlotId(slotId)
    setBookError('')
    setBookMessage('')
    try {
      await bookLesson(slotId)
      setTutor((prev) =>
        prev
          ? {
              ...prev,
              availableSlots: (prev.availableSlots ?? []).filter((slot) => slot.id !== slotId),
            }
          : prev,
      )
      setBookMessage('Rezerwacja zapisana. Zobacz „Moje lekcje”.')
    } catch (err) {
      const msg = err?.response?.data?.message
      setBookError(msg || 'Nie udało się zarezerwować terminu.')
    } finally {
      setBookingSlotId(null)
    }
  }

  if (!tutor && loadError) {
    return (
      <main className="page-shell">
        <section className="card">
          <p className="error" role="alert">
            {loadError}
          </p>
          <Link className="btn secondary" to="/search">
            Wróć do wyszukiwania
          </Link>
        </section>
      </main>
    )
  }

  if (!tutor) {
    return (
      <main className="page-shell">
        <section className="card centered loading-state">
          <p>Ładowanie profilu…</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <section className="card">
        <button type="button" className="btn ghost back-link" onClick={() => navigate(-1)}>
          ← Wstecz
        </button>
        <div className="tutor-detail-hero">
          <div>
            <h1>{[tutor.firstName, tutor.lastName].filter(Boolean).join(' ') || '—'}</h1>
            <p className="page-lead" style={{ marginTop: 8 }}>
              {tutor.minHourlyRate != null && tutor.maxHourlyRate != null ? (
                tutor.minHourlyRate === tutor.maxHourlyRate ? (
                  <strong>{formatPln(tutor.minHourlyRate)} PLN/h</strong>
                ) : (
                  <>
                    <strong>
                      {formatPln(tutor.minHourlyRate)}–{formatPln(tutor.maxHourlyRate)} PLN/h
                    </strong>
                  </>
                )
              ) : (
                <strong>{formatPln(tutor.hourlyRate)} PLN/h</strong>
              )}
            </p>
            {Array.isArray(tutor.offerings) && tutor.offerings.length > 0 && (
              <div className="tutor-meta-grid" style={{ marginTop: 12 }}>
                <p style={{ gridColumn: '1 / -1' }}>
                  <strong>Oferta</strong>
                </p>
                <ul className="muted" style={{ margin: 0, paddingLeft: '1.1rem', gridColumn: '1 / -1' }}>
                  {tutor.offerings.map((o, idx) => (
                    <li key={`${o.subjectId}-${idx}`} style={{ marginBottom: 6 }}>
                      {subjectById.get(o.subjectId) ?? `Przedmiot #${o.subjectId}`} — {teachingModeLabel(o.teachingMode)}
                      {(o.teachingMode === 1 || o.teachingMode === 2) && o.location ? `, ${o.location}` : ''} —{' '}
                      {formatPln(o.hourlyRate)} PLN/h, {o.durationMinutes} min, poziomy:{' '}
                      {formatSubjectNames(o.teachingLevelIds, levelById)}
                    </li>
                  ))}
                </ul>
              </div>
            )}
            <div className="tutor-meta-grid" style={{ marginTop: 18 }}>
              <p>
                <strong>Przedmioty</strong>
                {formatSubjectNames(tutor.subjectIds, subjectById)}
              </p>
              <p>
                <strong>Poziomy</strong>
                {formatSubjectNames(tutor.teachingLevelIds, levelById)}
              </p>
              <div className="rating-badge" aria-label="Średnia ocen">
                {avgRating === null
                  ? 'Brak opinii'
                  : `★ ${avgRating} · ${reviews.length} ${reviews.length === 1 ? 'opinia' : 'opinii'}`}
              </div>
            </div>
            {isStudent && (
              <button
                type="button"
                className="btn secondary"
                style={{ marginTop: 18 }}
                onClick={() => navigate('/conversations', { state: { tutorProfileId } })}
              >
                Napisz do tutora
              </button>
            )}
          </div>
          {tutor.photoUrl ? (
            <img
              className="tutor-detail-photo"
              src={resolveMediaUrl(tutor.photoUrl) ?? ''}
              alt=""
            />
          ) : null}
        </div>
      </section>

      <section className="card">
        <h2>Dostępne terminy</h2>
        {bookError && (
          <p className="error" role="alert">
            {bookError}
          </p>
        )}
        {bookMessage && (
          <p className="success" role="status">
            {bookMessage}
          </p>
        )}
        {!isStudent && (
          <p className="muted">Zaloguj się jako uczeń (Student), aby zarezerwować termin.</p>
        )}
        {futureSlots.length === 0 && <p className="muted">Brak przyszłych slotów w tym wyniku wyszukiwania.</p>}
        <div className="booking-calendar-controls">
          <label htmlFor="slotSubjectFilter">
            Przedmiot
            <select
              id="slotSubjectFilter"
              value={slotSubjectFilter}
              onChange={(event) => setSlotSubjectFilter(event.target.value)}
            >
              <option value="">Wszystkie</option>
              {[...new Set((tutor.offerings ?? []).map((offering) => offering.subjectId))].map((subjectId) => (
                <option key={subjectId} value={subjectId}>
                  {subjectById.get(subjectId) ?? `Przedmiot #${subjectId}`}
                </option>
              ))}
            </select>
          </label>
          <div className="booking-calendar-week-nav">
            <button type="button" className="btn secondary" onClick={() => setWeekStart((date) => addDays(date, -7))}>
              Poprzedni tydzień
            </button>
            <strong>
              {weekStart.toLocaleDateString('pl-PL', { dateStyle: 'medium' })} -{' '}
              {addDays(weekStart, 6).toLocaleDateString('pl-PL', { dateStyle: 'medium' })}
            </strong>
            <button type="button" className="btn secondary" onClick={() => setWeekStart((date) => addDays(date, 7))}>
              Następny tydzień
            </button>
          </div>
        </div>
        <div className="booking-calendar-week">
          {weekDays.map((day) => {
            const key = toDateKey(day)
            const daySlots = (slotsByDay.get(key) ?? []).sort(
              (a, b) => new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime(),
            )
            return (
              <section key={key} className="booking-calendar-day">
                <h3>
                  {day.toLocaleDateString('pl-PL', { weekday: 'short' })}
                  <span>{day.toLocaleDateString('pl-PL', { day: '2-digit', month: '2-digit' })}</span>
                </h3>
                {daySlots.length === 0 ? (
                  <p className="muted">Brak slotów</p>
                ) : (
                  <div className="booking-slot-buttons">
                    {daySlots.map((slot) => (
                      <button
                        key={slot.id}
                        type="button"
                        className="booking-slot-button"
                        disabled={!isStudent || bookingSlotId === slot.id}
                        onClick={() => handleBook(slot.id)}
                      >
                        <strong>
                          {new Date(slot.startUtc).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' })}
                          {' - '}
                          {new Date(slot.endUtc).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' })}
                        </strong>
                        <span>{subjectById.get(slot.subjectId) ?? `#${slot.subjectId}`}</span>
                      </button>
                    ))}
                  </div>
                )}
              </section>
            )
          })}
        </div>
      </section>

      {reviews.length > 0 && (
        <section className="card">
          <h2>Opinie uczniów</h2>
          <ul className="review-list">
            {reviews.map((r) => (
              <li key={r.id} className="review-item">
                <p>
                  <span className="rating-badge" style={{ marginBottom: 8 }}>
                    {r.rating}/5
                  </span>
                  <span className="muted"> · {formatWhen(r.createdAtUtc)}</span>
                </p>
                {r.comment && <p className="tutor-card__meta">{r.comment}</p>}
              </li>
            ))}
          </ul>
        </section>
      )}
    </main>
  )
}

export default TutorDetailPage
