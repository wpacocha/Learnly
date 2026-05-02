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
  const [fetchedTutor, setFetchedTutor] = useState(null)
  const tutor = seededOk ? seeded : fetchedTutor
  const [reviews, setReviews] = useState([])
  const [subjects, setSubjects] = useState([])
  const [levels, setLevels] = useState([])
  const [loadError, setLoadError] = useState('')
  const [bookingSlotId, setBookingSlotId] = useState(null)
  const [bookMessage, setBookMessage] = useState('')
  const [bookError, setBookError] = useState('')

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
    if (!tutorProfileId || seededOk) {
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
          setFetchedTutor(null)
          setLoadError('Nie znaleziono tutora. Wróć do wyszukiwania.')
          return
        }
        setFetchedTutor(found)
      } catch {
        if (!cancelled) {
          setFetchedTutor(null)
          setLoadError('Nie udało się załadować profilu tutora.')
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [tutorProfileId, seededOk])

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
        <ul className="slot-list">
          {futureSlots.map((slot) => (
            <li key={slot.id} className="slot-row">
              <span>
                {subjectById.get(slot.subjectId) ? `${subjectById.get(slot.subjectId)} · ` : ''}
                {formatWhen(slot.startUtc)} — {formatWhen(slot.endUtc)}
              </span>
              <button
                type="button"
                className="btn primary"
                disabled={!isStudent || bookingSlotId === slot.id}
                onClick={() => handleBook(slot.id)}
              >
                {bookingSlotId === slot.id ? 'Rezerwacja...' : 'Rezerwuj'}
              </button>
            </li>
          ))}
        </ul>
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
