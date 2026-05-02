import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { createReview } from '../api/reviewsApi'
import { fetchMyLessons, updateLessonStatus } from '../api/lessonsApi'
import { useAuth } from '../context/AuthContext'

const STATUS_LABEL = {
  0: 'Oczekuje',
  1: 'Potwierdzona',
  2: 'Zakończona',
  3: 'Anulowana',
}

const STATUS_PILL_CLASS = {
  0: 'status-pill--pending',
  1: 'status-pill--confirmed',
  2: 'status-pill--done',
  3: 'status-pill--cancelled',
}

const formatWhen = (iso) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const LessonsPage = () => {
  const { user } = useAuth()
  const [lessons, setLessons] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [busyId, setBusyId] = useState(null)
  const [reviewLessonId, setReviewLessonId] = useState(null)
  const [reviewRating, setReviewRating] = useState(5)
  const [reviewComment, setReviewComment] = useState('')
  const [reviewError, setReviewError] = useState('')
  const [reviewOk, setReviewOk] = useState('')

  const isStudent = user?.roles?.includes('Student')

  useEffect(() => {
    let cancelled = false

    ;(async () => {
      setIsLoading(true)
      setError('')
      try {
        const data = await fetchMyLessons()
        if (!cancelled) {
          setLessons(Array.isArray(data) ? data : [])
        }
      } catch {
        if (!cancelled) {
          setError('Nie udało się pobrać lekcji.')
          setLessons([])
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false)
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [])

  const reloadLessons = async () => {
    try {
      const data = await fetchMyLessons()
      setLessons(Array.isArray(data) ? data : [])
    } catch {
      setError('Nie udało się odświeżyć lekcji.')
    }
  }

  const patchStatus = async (lessonId, status) => {
    setBusyId(lessonId)
    try {
      await updateLessonStatus(lessonId, status)
      await reloadLessons()
    } catch (err) {
      const msg = err?.response?.data?.message
      setError(msg || 'Zmiana statusu nie powiodła się.')
    } finally {
      setBusyId(null)
    }
  }

  const submitReview = async (event) => {
    event.preventDefault()
    if (!reviewLessonId) {
      return
    }
    setReviewError('')
    setReviewOk('')
    try {
      await createReview({
        lessonId: reviewLessonId,
        rating: reviewRating,
        comment: reviewComment.trim() || null,
      })
      setReviewOk('Dziękujemy za opinię.')
      setReviewLessonId(null)
      setReviewComment('')
    } catch (err) {
      const msg = err?.response?.data?.message
      setReviewError(msg || 'Nie udało się zapisać opinii.')
    }
  }

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered loading-state">
          <p>Ładowanie lekcji…</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <section className="card">
        <h1>Moje lekcje</h1>
        <p className="page-lead">
          Jako uczeń lub tutor widzisz lekcje powiązane z kontem. Status i szczegóły są zawsze aktualne.
        </p>
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
        {lessons.length === 0 && <p className="muted">Nie masz jeszcze żadnych lekcji.</p>}
        <ul className="lessons-list">
          {lessons.map((lesson) => (
            <li key={lesson.id} className="lesson-card">
              <div>
                <p style={{ marginBottom: 10 }}>
                  <span
                    className={`status-pill ${STATUS_PILL_CLASS[lesson.status] ?? 'status-pill--pending'}`}
                  >
                    {STATUS_LABEL[lesson.status] ?? lesson.status}
                  </span>
                </p>
                <p>
                  <strong>Start:</strong> {formatWhen(lesson.startUtc)} — <strong>Koniec:</strong>{' '}
                  {formatWhen(lesson.endUtc)}
                </p>
                <p className="muted" style={{ fontSize: '0.8125rem' }}>
                  Identyfikator: {lesson.id}
                </p>
              </div>
              <div className="lesson-actions">
                <Link className="btn secondary" to={`/lessons/${lesson.id}/room`}>
                  Pokój lekcyjny
                </Link>
                {lesson.status !== 3 && (
                  <button
                    type="button"
                    className="btn ghost"
                    disabled={busyId === lesson.id}
                    onClick={() => patchStatus(lesson.id, 'Cancelled')}
                  >
                    Anuluj
                  </button>
                )}
                {lesson.status !== 2 && lesson.status !== 3 && (
                  <button
                    type="button"
                    className="btn secondary"
                    disabled={busyId === lesson.id}
                    onClick={() => patchStatus(lesson.id, 'Completed')}
                  >
                    Oznacz jako zakończoną
                  </button>
                )}
                {isStudent && lesson.status === 2 && (
                  <button type="button" className="btn primary" onClick={() => setReviewLessonId(lesson.id)}>
                    Oceń lekcję
                  </button>
                )}
              </div>
            </li>
          ))}
        </ul>
      </section>

      {reviewLessonId && (
        <section className="card">
          <h2>Opinia po lekcji</h2>
          {reviewError && (
            <p className="error" role="alert">
              {reviewError}
            </p>
          )}
          {reviewOk && (
            <p className="success" role="status">
              {reviewOk}
            </p>
          )}
          <form className="form" onSubmit={submitReview}>
            <label htmlFor="rating">Ocena (1–5)</label>
            <select id="rating" value={reviewRating} onChange={(e) => setReviewRating(Number(e.target.value))}>
              {[1, 2, 3, 4, 5].map((n) => (
                <option key={n} value={n}>
                  {n}
                </option>
              ))}
            </select>
            <label htmlFor="comment">Komentarz (opcjonalnie)</label>
            <textarea
              id="comment"
              rows={3}
              value={reviewComment}
              onChange={(e) => setReviewComment(e.target.value)}
              maxLength={2000}
            />
            <div className="lesson-actions">
              <button className="btn primary" type="submit">
                Wyślij opinię
              </button>
              <button type="button" className="btn ghost" onClick={() => setReviewLessonId(null)}>
                Anuluj
              </button>
            </div>
          </form>
        </section>
      )}
    </main>
  )
}

export default LessonsPage
