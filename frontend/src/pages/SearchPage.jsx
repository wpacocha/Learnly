import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchSubjects, fetchTeachingLevels } from '../api/catalogApi'
import { searchTutors } from '../api/tutorSearchApi'
import { useLanguage } from '../context/LanguageContext'

const RATE_SLIDER_MAX = 500

const emptyFilters = {
  subjectId: '',
  teachingLevelId: '',
  teachingMode: '',
  location: '',
  availableFrom: '',
  rateMin: 0,
  rateMax: RATE_SLIDER_MAX,
  minRating: '',
}

const teachingModeLabel = (m) => {
  if (m === 0) return 'Online'
  if (m === 1) return 'Stacjonarnie'
  return 'Online i stacjonarnie'
}

const formatNames = (ids, byId, fallback) =>
  ids?.length ? ids.map((id) => byId.get(id) ?? `${fallback} #${id}`).join(', ') : '-'

const formatPln = (value) =>
  Number(value).toLocaleString('pl-PL', { minimumFractionDigits: 0, maximumFractionDigits: 2 })

const formatRating = (value) =>
  Number(value).toLocaleString('pl-PL', { minimumFractionDigits: 1, maximumFractionDigits: 1 })

const formatResultCount = (n, t) => {
  if (n === 0) return t('Brak wynikow przy tych kryteriach.')
  if (document.documentElement.lang === 'pl') {
    if (n === 1) return '1 wynik'
    const mod100 = n % 100
    const mod10 = n % 10
    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 > 20)) return `${n} wyniki`
    return `${n} wynikow`
  }
  if (n === 1) return '1 result'
  return `${n} results`
}

const buildSearchParams = (f) => {
  const params = {}
  if (f.subjectId) params.subjectId = Number(f.subjectId)
  if (f.teachingLevelId) params.teachingLevelId = Number(f.teachingLevelId)
  if (f.teachingMode !== '') params.teachingMode = Number(f.teachingMode)
  if (f.location.trim() && f.teachingMode !== '0') params.location = f.location.trim()
  if (f.availableFrom) params.availableFromUtc = new Date(f.availableFrom).toISOString()
  if (f.minRating) params.minRating = Number(f.minRating)
  return params
}

const applyRateFilters = (list, f) => {
  let next = list
  if (f.rateMin > 0) {
    next = next.filter((tutor) => (tutor.maxHourlyRate ?? tutor.hourlyRate ?? 0) >= f.rateMin)
  }
  if (f.rateMax < RATE_SLIDER_MAX) {
    next = next.filter((tutor) => (tutor.minHourlyRate ?? tutor.hourlyRate ?? 0) <= f.rateMax)
  }
  return next
}

const SearchPage = () => {
  const { t } = useLanguage()
  const [subjects, setSubjects] = useState([])
  const [levels, setLevels] = useState([])
  const [filters, setFilters] = useState(emptyFilters)
  const [results, setResults] = useState([])
  const [isLoadingCatalog, setIsLoadingCatalog] = useState(true)
  const [isSearching, setIsSearching] = useState(false)
  const [error, setError] = useState('')

  const subjectById = useMemo(() => new Map(subjects.map((s) => [s.id, t(s.name)])), [subjects, t])
  const levelById = useMemo(() => new Map(levels.map((l) => [l.id, t(l.name)])), [levels, t])

  const runSearch = useCallback(
    async (f) => {
      setIsSearching(true)
      setError('')
      try {
        const data = await searchTutors(buildSearchParams(f))
        const list = Array.isArray(data) ? data : []
        setResults(applyRateFilters(list, f))
      } catch {
        setError(t('Wyszukiwanie nie powiodlo sie. Sprawdz, czy API dziala.'))
        setResults([])
      } finally {
        setIsSearching(false)
      }
    },
    [t],
  )

  useEffect(() => {
    let cancelled = false

    ;(async () => {
      setIsLoadingCatalog(true)
      setError('')
      try {
        const [catalogSubjects, catalogLevels] = await Promise.all([fetchSubjects(), fetchTeachingLevels()])
        if (cancelled) return
        setSubjects(catalogSubjects)
        setLevels(catalogLevels)
        await runSearch(emptyFilters)
      } catch {
        if (!cancelled) {
          setError(t('Nie udalo sie pobrac katalogu przedmiotow i poziomow.'))
        }
      } finally {
        if (!cancelled) {
          setIsLoadingCatalog(false)
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [runSearch, t])

  const handleFilterChange = (event) => {
    const { name, value } = event.target
    setFilters((prev) => {
      const next = { ...prev, [name]: value }
      if (name === 'teachingMode' && value === '0') next.location = ''
      return next
    })
  }

  const handleSubmit = (event) => {
    event.preventDefault()
    runSearch(filters)
  }

  const handleResetFilters = () => {
    setFilters(emptyFilters)
    runSearch(emptyFilters)
  }

  if (isLoadingCatalog) {
    return (
      <main className="page-shell search-page-shell">
        <section className="card centered loading-state">
          <p>{t('Ladowanie katalogu...')}</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell search-page-shell">
      <div className="search-page-intro">
        <h1>{t('Znajdz tutora')}</h1>
        <p className="page-lead">{t('Uzyj filtrow ponizej, a pod nimi zobaczysz dopasowanych korepetytorow.')}</p>
      </div>

      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}

      <form className="search-toolbar" onSubmit={handleSubmit}>
        <div className="search-toolbar__main">
          <label className="search-field search-field--subject" htmlFor="subjectId">
            <span className="search-field__label">{t('Przedmiot')}</span>
            <select id="subjectId" name="subjectId" value={filters.subjectId} onChange={handleFilterChange}>
              <option value="">{t('Wszystkie')}</option>
              {subjects.map((s) => (
                <option key={s.id} value={s.id}>
                  {t(s.name)}
                </option>
              ))}
            </select>
          </label>

          <label className="search-field search-field--level" htmlFor="teachingLevelId">
            <span className="search-field__label">{t('Poziom')}</span>
            <select
              id="teachingLevelId"
              name="teachingLevelId"
              value={filters.teachingLevelId}
              onChange={handleFilterChange}
            >
              <option value="">{t('Wszystkie')}</option>
              {levels.map((l) => (
                <option key={l.id} value={l.id}>
                  {t(l.name)}
                </option>
              ))}
            </select>
          </label>

          <label className="search-field search-field--mode" htmlFor="teachingMode">
            <span className="search-field__label">{t('Tryb')}</span>
            <select id="teachingMode" name="teachingMode" value={filters.teachingMode} onChange={handleFilterChange}>
              <option value="">{t('Dowolny')}</option>
              <option value="0">Online</option>
              <option value="1">{t('Stacjonarnie')}</option>
              <option value="2">{t('Online i stacjonarnie')}</option>
            </select>
          </label>

          {filters.teachingMode !== '0' && (
            <label className="search-field search-field--location" htmlFor="location">
              <span className="search-field__label">{t('Lokalizacja')}</span>
              <input
                id="location"
                name="location"
                type="text"
                value={filters.location}
                onChange={handleFilterChange}
                placeholder={
                  filters.teachingMode === ''
                    ? t('Opcjonalnie - glownie dla stacjonarnie / hybryda')
                    : t('np. Warszawa')
                }
                autoComplete="off"
              />
            </label>
          )}
        </div>

        <div className="search-toolbar__secondary">
          <label className="search-field search-field--date" htmlFor="availableFrom">
            <span className="search-field__label">{t('Dostepny od')}</span>
            <input
              id="availableFrom"
              name="availableFrom"
              type="datetime-local"
              value={filters.availableFrom}
              onChange={handleFilterChange}
            />
          </label>

          <label className="search-field search-field--rating" htmlFor="minRating">
            <span className="search-field__label">{t('Ocena tutora')}</span>
            <select id="minRating" name="minRating" value={filters.minRating} onChange={handleFilterChange}>
              <option value="">{t('Dowolna')}</option>
              <option value="5">5.0</option>
              <option value="4.5">4.5+</option>
              <option value="4">4.0+</option>
              <option value="3">3.0+</option>
            </select>
          </label>

          <div className="search-toolbar__rate">
            <div className="rate-range" role="group" aria-label={t('Zakres stawki godzinowej')}>
              <div className="rate-range-header">
                <span>{t('Stawka (PLN/h)')}</span>
                <strong>
                  {filters.rateMin === 0 && filters.rateMax === RATE_SLIDER_MAX
                    ? t('Dowolna')
                    : `${filters.rateMin} - ${filters.rateMax} PLN`}
                </strong>
              </div>
              <div className="rate-range-sliders">
                <input
                  aria-label={t('Minimalna stawka')}
                  type="range"
                  min={0}
                  max={RATE_SLIDER_MAX}
                  step={5}
                  value={filters.rateMin}
                  onChange={(event) =>
                    setFilters((prev) => ({ ...prev, rateMin: Math.min(Number(event.target.value), prev.rateMax) }))
                  }
                />
                <input
                  aria-label={t('Maksymalna stawka')}
                  type="range"
                  min={0}
                  max={RATE_SLIDER_MAX}
                  step={5}
                  value={filters.rateMax}
                  onChange={(event) =>
                    setFilters((prev) => ({ ...prev, rateMax: Math.max(Number(event.target.value), prev.rateMin) }))
                  }
                />
              </div>
            </div>
          </div>

          <div className="search-toolbar__actions">
            <button className="btn primary" type="submit" disabled={isSearching}>
              {isSearching ? t('Szukam...') : t('Szukaj')}
            </button>
            <button type="button" className="btn secondary" onClick={handleResetFilters}>
              {t('Wyczysc filtry')}
            </button>
          </div>
        </div>
      </form>

      <div className="search-results-bar">
        <span className="search-results-bar__count">{formatResultCount(results.length, t)}</span>
      </div>

      <section className="tutor-grid search-results-grid" aria-label={t('Wyniki wyszukiwania')}>
        {results.map((tutor) => (
          <article className="card tutor-card" key={tutor.tutorProfileId}>
            <div className="tutor-card__head">
              <h2 className="tutor-card__title">
                {[tutor.firstName, tutor.lastName].filter(Boolean).join(' ') || '-'}
              </h2>
              <span className="tutor-card__rate">
                {tutor.minHourlyRate != null && tutor.maxHourlyRate != null
                  ? tutor.minHourlyRate === tutor.maxHourlyRate
                    ? `${formatPln(tutor.minHourlyRate)} PLN/h`
                    : `${formatPln(tutor.minHourlyRate)}-${formatPln(tutor.maxHourlyRate)} PLN/h`
                  : `${formatPln(tutor.hourlyRate)} PLN/h`}
              </span>
            </div>
            <span className="rating-badge tutor-card__rating" aria-label={t('Ocena tutora')}>
              {tutor.averageRating == null
                ? t('Brak ocen')
                : `★ ${formatRating(tutor.averageRating)} · ${tutor.reviewCount} ${
                    tutor.reviewCount === 1 ? t('opinia') : t('opinii')
                  }`}
            </span>
            {Array.isArray(tutor.offerings) && tutor.offerings.length > 0 && (
              <ul className="tutor-card__offerings muted" style={{ margin: '0 0 8px', paddingLeft: '1.1rem' }}>
                {tutor.offerings.map((o, idx) => (
                  <li key={`${o.subjectId}-${idx}`} style={{ marginBottom: 4 }}>
                    <strong>{subjectById.get(o.subjectId) ?? `${t('Przedmiot')} #${o.subjectId}`}</strong>
                    {' · '}
                    {t(teachingModeLabel(o.teachingMode))}
                    {(o.teachingMode === 1 || o.teachingMode === 2) && o.location ? ` · ${o.location}` : ''}
                    {' · '}
                    {formatPln(o.hourlyRate)} PLN/h · {o.durationMinutes} min
                    {o.teachingLevelIds?.length
                      ? ` · ${t('poziomy:')} ${formatNames(o.teachingLevelIds, levelById, t('Poziom'))}`
                      : ''}
                  </li>
                ))}
              </ul>
            )}
            <p className="tutor-card__meta">
              <strong>{t('Przedmioty')}</strong>
              {formatNames(tutor.subjectIds, subjectById, t('Przedmiot'))}
            </p>
            <p className="tutor-card__meta">
              <strong>{t('Poziomy')}</strong>
              {formatNames(tutor.teachingLevelIds, levelById, t('Poziom'))}
            </p>
            <p className="tutor-card__meta muted">
              <strong>{t('Wolne sloty')}</strong>
              {tutor.availableSlots?.length ?? 0}
            </p>
            <Link className="btn primary" to={`/tutors/${tutor.tutorProfileId}`} state={{ tutor }}>
              {t('Profil i rezerwacja')}
            </Link>
          </article>
        ))}
        {!results.length && (
          <article className="card search-empty-card">
            <h2>{t('Brak wynikow')}</h2>
            <p className="muted">
              {t('Poszerz zakres stawki, usun filtr daty albo wybierz inny przedmiot. Upewnij sie tez, ze tutorzy maja wypelniona dostepnosc.')}
            </p>
          </article>
        )}
      </section>
    </main>
  )
}

export default SearchPage
