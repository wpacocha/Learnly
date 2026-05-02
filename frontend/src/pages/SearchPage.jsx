import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchSubjects, fetchTeachingLevels } from '../api/catalogApi'
import { searchTutors } from '../api/tutorSearchApi'

/** Górny limit suwaka stawki (PLN/h); pełny zakres = brak filtrowania po górnej granicy. */
const RATE_SLIDER_MAX = 500

const emptyFilters = {
  subjectId: '',
  teachingLevelId: '',
  teachingMode: '',
  location: '',
  availableFrom: '',
  rateMin: 0,
  rateMax: RATE_SLIDER_MAX,
}

const teachingModeLabel = (m) => {
  if (m === 0) return 'Online'
  if (m === 1) return 'Stacjonarnie'
  return 'Online i stacjonarnie'
}

const formatSubjectNames = (ids, byId) =>
  ids?.length ? ids.map((id) => byId.get(id) ?? `#${id}`).join(', ') : '—'

const formatPln = (value) =>
  Number(value).toLocaleString('pl-PL', { minimumFractionDigits: 0, maximumFractionDigits: 2 })

const formatWynikCount = (n) => {
  if (n === 1) return '1 wynik'
  const mod100 = n % 100
  const mod10 = n % 10
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 > 20)) {
    return `${n} wyniki`
  }
  return `${n} wyników`
}

const buildSearchParams = (f) => {
  const params = {}
  if (f.subjectId) {
    params.subjectId = Number(f.subjectId)
  }
  if (f.teachingLevelId) {
    params.teachingLevelId = Number(f.teachingLevelId)
  }
  if (f.teachingMode !== '') {
    params.teachingMode = Number(f.teachingMode)
  }
  if (f.location.trim() && f.teachingMode !== '0') {
    params.location = f.location.trim()
  }
  if (f.availableFrom) {
    params.availableFromUtc = new Date(f.availableFrom).toISOString()
  }
  return params
}

const applyRateFilters = (list, f) => {
  let next = list
  if (f.rateMin > 0) {
    next = next.filter((t) => (t.maxHourlyRate ?? t.hourlyRate ?? 0) >= f.rateMin)
  }
  if (f.rateMax < RATE_SLIDER_MAX) {
    next = next.filter((t) => (t.minHourlyRate ?? t.hourlyRate ?? 0) <= f.rateMax)
  }
  return next
}

const SearchPage = () => {
  const [subjects, setSubjects] = useState([])
  const [levels, setLevels] = useState([])
  const [filters, setFilters] = useState(emptyFilters)
  const [results, setResults] = useState([])
  const [isLoadingCatalog, setIsLoadingCatalog] = useState(true)
  const [isSearching, setIsSearching] = useState(false)
  const [error, setError] = useState('')

  const subjectById = useMemo(() => new Map(subjects.map((s) => [s.id, s.name])), [subjects])
  const levelById = useMemo(() => new Map(levels.map((l) => [l.id, l.name])), [levels])

  const runSearch = useCallback(async (f) => {
    setIsSearching(true)
    setError('')
    try {
      const data = await searchTutors(buildSearchParams(f))
      const list = Array.isArray(data) ? data : []
      setResults(applyRateFilters(list, f))
    } catch {
      setError('Wyszukiwanie nie powiodło się. Sprawdź, czy API działa.')
      setResults([])
    } finally {
      setIsSearching(false)
    }
  }, [])

  useEffect(() => {
    let cancelled = false

    ;(async () => {
      setIsLoadingCatalog(true)
      setError('')
      try {
        const [s, l] = await Promise.all([fetchSubjects(), fetchTeachingLevels()])
        if (cancelled) {
          return
        }
        setSubjects(s)
        setLevels(l)
        await runSearch(emptyFilters)
      } catch {
        if (!cancelled) {
          setError('Nie udało się pobrać katalogu przedmiotów i poziomów.')
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
  }, [runSearch])

  const handleFilterChange = (event) => {
    const { name, value } = event.target
    setFilters((prev) => {
      const next = { ...prev, [name]: value }
      if (name === 'teachingMode' && value === '0') {
        next.location = ''
      }
      return next
    })
  }

  const handleRateMinChange = (event) => {
    const v = Number(event.target.value)
    setFilters((prev) => ({ ...prev, rateMin: Math.min(v, prev.rateMax) }))
  }

  const handleRateMaxChange = (event) => {
    const v = Number(event.target.value)
    setFilters((prev) => ({ ...prev, rateMax: Math.max(v, prev.rateMin) }))
  }

  const handleSubmit = (event) => {
    event.preventDefault()
    runSearch(filters)
  }

  const handleResetFilters = () => {
    setFilters(emptyFilters)
    runSearch(emptyFilters)
  }

  const showLocationField = filters.teachingMode !== '0'

  if (isLoadingCatalog) {
    return (
      <main className="page-shell search-page-shell">
        <section className="card centered loading-state">
          <p>Ładowanie katalogu…</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell search-page-shell">
      <div className="search-page-intro">
        <h1>Znajdź tutora</h1>
        <p className="page-lead">
          Użyj filtrów poniżej, a pod nimi zobaczysz dopasowanych korepetytorów.
        </p>
      </div>

      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}

      <form className="search-toolbar" onSubmit={handleSubmit}>
        <div className="search-toolbar__main">
          <label className="search-field search-field--subject" htmlFor="subjectId">
            <span className="search-field__label">Przedmiot</span>
            <select id="subjectId" name="subjectId" value={filters.subjectId} onChange={handleFilterChange}>
              <option value="">Wszystkie</option>
              {subjects.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
          </label>

          <label className="search-field search-field--level" htmlFor="teachingLevelId">
            <span className="search-field__label">Poziom</span>
            <select
              id="teachingLevelId"
              name="teachingLevelId"
              value={filters.teachingLevelId}
              onChange={handleFilterChange}
            >
              <option value="">Wszystkie</option>
              {levels.map((l) => (
                <option key={l.id} value={l.id}>
                  {l.name}
                </option>
              ))}
            </select>
          </label>

          <label className="search-field search-field--mode" htmlFor="teachingMode">
            <span className="search-field__label">Tryb</span>
            <select
              id="teachingMode"
              name="teachingMode"
              value={filters.teachingMode}
              onChange={handleFilterChange}
            >
              <option value="">Dowolny</option>
              <option value="0">Online</option>
              <option value="1">Stacjonarnie</option>
              <option value="2">Online i stacjonarnie</option>
            </select>
          </label>

          {showLocationField && (
            <label className="search-field search-field--location" htmlFor="location">
              <span className="search-field__label">Lokalizacja</span>
              <input
                id="location"
                name="location"
                type="text"
                value={filters.location}
                onChange={handleFilterChange}
                placeholder={
                  filters.teachingMode === ''
                    ? 'Opcjonalnie — głównie dla stacjonarnie / hybryda'
                    : 'np. Warszawa'
                }
                autoComplete="off"
              />
            </label>
          )}
        </div>

        <div className="search-toolbar__secondary">
          <label className="search-field search-field--date" htmlFor="availableFrom">
            <span className="search-field__label">Dostępny od</span>
            <input
              id="availableFrom"
              name="availableFrom"
              type="datetime-local"
              value={filters.availableFrom}
              onChange={handleFilterChange}
            />
          </label>

          <div className="search-toolbar__rate">
            <div className="rate-range" role="group" aria-label="Zakres stawki godzinowej">
              <div className="rate-range-header">
                <span>Stawka (PLN/h)</span>
                <strong>
                  {filters.rateMin === 0 && filters.rateMax === RATE_SLIDER_MAX
                    ? 'Dowolna'
                    : `${filters.rateMin} – ${filters.rateMax} PLN`}
                </strong>
              </div>
              <div className="rate-range-sliders">
                <input
                  aria-label="Minimalna stawka"
                  type="range"
                  min={0}
                  max={RATE_SLIDER_MAX}
                  step={5}
                  value={filters.rateMin}
                  onChange={handleRateMinChange}
                />
                <input
                  aria-label="Maksymalna stawka"
                  type="range"
                  min={0}
                  max={RATE_SLIDER_MAX}
                  step={5}
                  value={filters.rateMax}
                  onChange={handleRateMaxChange}
                />
              </div>
            </div>
          </div>

          <div className="search-toolbar__actions">
            <button className="btn primary" type="submit" disabled={isSearching}>
              {isSearching ? 'Szukam…' : 'Szukaj'}
            </button>
            <button type="button" className="btn secondary" onClick={handleResetFilters}>
              Wyczyść filtry
            </button>
          </div>
        </div>
      </form>

      <div className="search-results-bar">
        <span className="search-results-bar__count">
          {results.length === 0
            ? 'Brak wyników przy tych kryteriach.'
            : formatWynikCount(results.length)}
        </span>
      </div>

      <section className="tutor-grid search-results-grid" aria-label="Wyniki wyszukiwania">
        {results.map((tutor) => (
          <article className="card tutor-card" key={tutor.tutorProfileId}>
            <div className="tutor-card__head">
              <h2 className="tutor-card__title">
                {[tutor.firstName, tutor.lastName].filter(Boolean).join(' ') || '—'}
              </h2>
              <span className="tutor-card__rate">
                {tutor.minHourlyRate != null && tutor.maxHourlyRate != null
                  ? tutor.minHourlyRate === tutor.maxHourlyRate
                    ? `${formatPln(tutor.minHourlyRate)} PLN/h`
                    : `${formatPln(tutor.minHourlyRate)}–${formatPln(tutor.maxHourlyRate)} PLN/h`
                  : `${formatPln(tutor.hourlyRate)} PLN/h`}
              </span>
            </div>
            {Array.isArray(tutor.offerings) && tutor.offerings.length > 0 && (
              <ul className="tutor-card__offerings muted" style={{ margin: '0 0 8px', paddingLeft: '1.1rem' }}>
                {tutor.offerings.map((o, idx) => (
                  <li key={`${o.subjectId}-${idx}`} style={{ marginBottom: 4 }}>
                    <strong>{subjectById.get(o.subjectId) ?? `Przedmiot #${o.subjectId}`}</strong>
                    {' · '}
                    {teachingModeLabel(o.teachingMode)}
                    {(o.teachingMode === 1 || o.teachingMode === 2) && o.location ? ` · ${o.location}` : ''}
                    {' · '}
                    {formatPln(o.hourlyRate)} PLN/h · {o.durationMinutes} min
                  </li>
                ))}
              </ul>
            )}
            <p className="tutor-card__meta">
              <strong>Przedmioty</strong>
              {formatSubjectNames(tutor.subjectIds, subjectById)}
            </p>
            <p className="tutor-card__meta">
              <strong>Poziomy</strong>
              {formatSubjectNames(tutor.teachingLevelIds, levelById)}
            </p>
            <p className="tutor-card__meta muted">
              <strong>Wolne sloty</strong>
              {tutor.availableSlots?.length ?? 0}
            </p>
            <Link className="btn primary" to={`/tutors/${tutor.tutorProfileId}`} state={{ tutor }}>
              Profil i rezerwacja
            </Link>
          </article>
        ))}
        {!results.length && (
          <article className="card search-empty-card">
            <h2>Brak wyników</h2>
            <p className="muted">
              Poszerz zakres stawki, usuń filtr daty albo wybierz inny przedmiot. Upewnij się też, że tutorzy mają
              wypełnioną dostępność.
            </p>
          </article>
        )}
      </section>
    </main>
  )
}

export default SearchPage
