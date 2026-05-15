import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchMyLessons, updateLessonStatus } from '../api/lessonsApi'
import { createReview } from '../api/reviewsApi'
import { useAuth } from '../context/AuthContext'
import { useLanguage } from '../context/LanguageContext'

const STATUS_LABEL = {
  0: 'Oczekuje',
  1: 'Potwierdzona',
  2: 'Zakonczona',
  3: 'Anulowana',
}

const STATUS_PILL_CLASS = {
  0: 'status-pill--pending',
  1: 'status-pill--confirmed',
  2: 'status-pill--done',
  3: 'status-pill--cancelled',
}

const REVIEW_RATINGS = Array.from({ length: 9 }, (_, i) => 1 + i * 0.5)

const formatWhen = (iso) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const getStarFill = (rating, star) => {
  if (rating >= star) return '100%'
  if (rating >= star - 0.5) return '50%'
  return '0%'
}

const getRatingFromStarClick = (event, star) => {
  const rect = event.currentTarget.getBoundingClientRect()
  const isLeftHalf = event.clientX - rect.left < rect.width / 2
  if (star === 1) return 1
  return isLeftHalf ? star - 0.5 : star
}

const LessonsPage = () => {
  const { user } = useAuth()
  const { t } = useLanguage()
  const [lessons, setLessons] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [busyId, setBusyId] = useState(null)
  const [reviewLessonId, setReviewLessonId] = useState(null)
  const [reviewRating, setReviewRating] = useState(5)
  const [reviewComment, setReviewComment] = useState('')
  const [reviewError, setReviewError] = useState('')

  const isStudent = user?.roles?.includes('Student')
  const reviewLesson = lessons.find((lesson) => lesson.id === reviewLessonId)

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
          setError(t('Nie udalo sie pobrac lekcji.'))
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
  }, [t])

  const reloadLessons = async () => {
    try {
      const data = await fetchMyLessons()
      setLessons(Array.isArray(data) ? data : [])
    } catch {
      setError(t('Nie udalo sie odswiezyc lekcji.'))
    }
  }

  const patchStatus = async (lessonId, status) => {
    setBusyId(lessonId)
    try {
      await updateLessonStatus(lessonId, status)
      await reloadLessons()
    } catch (err) {
      const msg = err?.response?.data?.message
      setError(msg || t('Zmiana statusu nie powiodla sie.'))
    } finally {
      setBusyId(null)
    }
  }

  const closeReviewModal = () => {
    setReviewLessonId(null)
    setReviewRating(5)
    setReviewComment('')
    setReviewError('')
  }

  const submitReview = async (event) => {
    event.preventDefault()
    if (!reviewLessonId) return
    setReviewError('')
    try {
      await createReview({
        lessonId: reviewLessonId,
        rating: reviewRating,
        comment: reviewComment.trim() || null,
      })
      setLessons((prev) =>
        prev.map((lesson) => (lesson.id === reviewLessonId ? { ...lesson, hasReview: true } : lesson)),
      )
      closeReviewModal()
    } catch (err) {
      const msg = err?.response?.data?.message
      setReviewError(msg || t('Nie udalo sie zapisac opinii.'))
    }
  }

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered loading-state">
          <p>{t('Ladowanie lekcji...')}</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <section className="card">
        <h1>{t('Moje lekcje')}</h1>
        <p className="page-lead">
          {t('Jako uczen lub tutor widzisz lekcje powiazane z kontem. Status i szczegoly sa zawsze aktualne.')}
        </p>
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
        {lessons.length === 0 && <p className="muted">{t('Nie masz jeszcze zadnych lekcji.')}</p>}
        <ul className="lessons-list">
          {lessons.map((lesson) => (
            <li key={lesson.id} className="lesson-card">
              <div>
                <p style={{ marginBottom: 10 }}>
                  <span className={`status-pill ${STATUS_PILL_CLASS[lesson.status] ?? 'status-pill--pending'}`}>
                    {t(STATUS_LABEL[lesson.status] ?? String(lesson.status))}
                  </span>
                </p>
                <p>
                  <strong>{t('Start')}:</strong> {formatWhen(lesson.startUtc)} - <strong>{t('Koniec')}:</strong>{' '}
                  {formatWhen(lesson.endUtc)}
                </p>
                <p className="muted" style={{ fontSize: '0.8125rem' }}>
                  {t('Identyfikator')}: {lesson.id}
                </p>
              </div>
              <div className="lesson-actions">
                <Link className="btn secondary" to={`/lessons/${lesson.id}/room`}>
                  {t('Pokoj lekcyjny')}
                </Link>
                {lesson.status !== 3 && (
                  <button
                    type="button"
                    className="btn ghost"
                    disabled={busyId === lesson.id}
                    onClick={() => patchStatus(lesson.id, 'Cancelled')}
                  >
                    {t('Anuluj')}
                  </button>
                )}
                {lesson.status !== 2 && lesson.status !== 3 && (
                  <button
                    type="button"
                    className="btn secondary"
                    disabled={busyId === lesson.id}
                    onClick={() => patchStatus(lesson.id, 'Completed')}
                  >
                    {t('Oznacz jako zakonczona')}
                  </button>
                )}
                {isStudent && lesson.status === 2 && (
                  <button
                    type="button"
                    className={lesson.hasReview ? 'btn secondary' : 'btn primary'}
                    disabled={lesson.hasReview}
                    onClick={() => {
                      if (!lesson.hasReview) {
                        setReviewLessonId(lesson.id)
                      }
                    }}
                  >
                    {lesson.hasReview ? t('Wystawiono opinie') : t('Ocen lekcje')}
                  </button>
                )}
              </div>
            </li>
          ))}
        </ul>
      </section>

      {reviewLessonId && (
        <div className="modal-backdrop" role="presentation" onMouseDown={closeReviewModal}>
          <section
            className="modal-card review-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="review-modal-title"
            onMouseDown={(event) => event.stopPropagation()}
          >
            <h2 id="review-modal-title">{t('Ocen prowadzacego')}</h2>
            {reviewLesson && (
              <p className="muted">
                {t('Lekcja')}: {formatWhen(reviewLesson.startUtc)} - {formatWhen(reviewLesson.endUtc)}
              </p>
            )}
            {reviewError && (
              <p className="error" role="alert">
                {reviewError}
              </p>
            )}
            <form className="form" onSubmit={submitReview}>
              <fieldset className="star-rating-field">
                <legend>{t('Ocena')}</legend>
                <div className="star-rating" aria-label={`${t('Ocena')} ${reviewRating} z 5`}>
                  {[1, 2, 3, 4, 5].map((star) => (
                    <button
                      key={star}
                      type="button"
                      className="star-rating__star"
                      onClick={(event) => setReviewRating(getRatingFromStarClick(event, star))}
                      aria-label={`${t('Ustaw ocene do')} ${star}`}
                    >
                      <span className="star-rating__base">★</span>
                      <span className="star-rating__fill" style={{ width: getStarFill(reviewRating, star) }}>
                        ★
                      </span>
                    </button>
                  ))}
                </div>
                <span className="star-rating-value">{reviewRating.toLocaleString('pl-PL')} / 5</span>
              </fieldset>
              <label htmlFor="rating">{t('Ocena (1-5)')}</label>
              <select id="rating" value={reviewRating} onChange={(e) => setReviewRating(Number(e.target.value))}>
                {REVIEW_RATINGS.map((n) => (
                  <option key={n} value={n}>
                    {n.toLocaleString('pl-PL')}
                  </option>
                ))}
              </select>
              <label htmlFor="comment">{t('Komentarz (opcjonalnie)')}</label>
              <textarea
                id="comment"
                rows={3}
                value={reviewComment}
                onChange={(e) => setReviewComment(e.target.value)}
                maxLength={2000}
              />
              <div className="lesson-actions">
                <button className="btn primary" type="submit">
                  {t('Wyslij opinie')}
                </button>
                <button type="button" className="btn ghost" onClick={closeReviewModal}>
                  {t('Anuluj')}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
    </main>
  )
}

export default LessonsPage
