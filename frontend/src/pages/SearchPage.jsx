import { useMemo, useState } from 'react'

const mockedTutors = [
  {
    id: 1,
    name: 'Anna Kowalska',
    subject: 'Matematyka',
    level: 'Liceum',
    location: 'Warszawa',
    hourlyRate: 80,
    rating: 4.9,
  },
  {
    id: 2,
    name: 'Michał Nowak',
    subject: 'Język angielski',
    level: 'Egzamin ósmoklasisty',
    location: 'Kraków',
    hourlyRate: 70,
    rating: 4.7,
  },
  {
    id: 3,
    name: 'Karolina Wiśniewska',
    subject: 'Fizyka',
    level: 'Matura rozszerzona',
    location: 'Gdańsk',
    hourlyRate: 95,
    rating: 5.0,
  },
]

const SearchPage = () => {
  const rateBounds = useMemo(() => {
    const rates = mockedTutors.map((tutor) => tutor.hourlyRate)
    return {
      min: Math.min(...rates),
      max: Math.max(...rates),
    }
  }, [])

  const [filters, setFilters] = useState({
    subject: '',
    level: '',
    location: '',
    minRate: rateBounds.min,
    maxRate: rateBounds.max,
    minRating: '',
  })

  const subjects = useMemo(
    () => [...new Set(mockedTutors.map((tutor) => tutor.subject))].sort((a, b) => a.localeCompare(b)),
    [],
  )
  const levels = useMemo(
    () => [...new Set(mockedTutors.map((tutor) => tutor.level))].sort((a, b) => a.localeCompare(b)),
    [],
  )
  const rateRangeStyle = useMemo(() => {
    const total = rateBounds.max - rateBounds.min
    const minPercent = ((Number(filters.minRate) - rateBounds.min) / total) * 100
    const maxPercent = ((Number(filters.maxRate) - rateBounds.min) / total) * 100

    return {
      background: `linear-gradient(to right, #cbd5e1 0%, #cbd5e1 ${minPercent}%, #4f46e5 ${minPercent}%, #4f46e5 ${maxPercent}%, #cbd5e1 ${maxPercent}%, #cbd5e1 100%)`,
    }
  }, [filters.maxRate, filters.minRate, rateBounds.max, rateBounds.min])

  const filteredTutors = useMemo(() => {
    const normalizedLocation = filters.location.trim().toLowerCase()
    const minRate = Number(filters.minRate)
    const maxRate = Number(filters.maxRate)
    const minRating = Number(filters.minRating) || 0

    return mockedTutors.filter((tutor) => {
      const matchesSubject = !filters.subject || tutor.subject === filters.subject
      const matchesLevel = !filters.level || tutor.level === filters.level
      const matchesLocation =
        !normalizedLocation || tutor.location.toLowerCase().includes(normalizedLocation)
      const matchesRate = tutor.hourlyRate >= minRate && tutor.hourlyRate <= maxRate
      const matchesRating = tutor.rating >= minRating

      return matchesSubject && matchesLevel && matchesLocation && matchesRate && matchesRating
    })
  }, [filters])

  const handleFilterChange = (event) => {
    const { name, value } = event.target
    setFilters((currentFilters) => ({ ...currentFilters, [name]: value }))
  }

  const handleRateChange = (event) => {
    const { name, value } = event.target
    const numericValue = Number(value)

    setFilters((currentFilters) => {
      if (name === 'minRate') {
        return {
          ...currentFilters,
          minRate: Math.min(numericValue, Number(currentFilters.maxRate)),
        }
      }

      return {
        ...currentFilters,
        maxRate: Math.max(numericValue, Number(currentFilters.minRate)),
      }
    })
  }

  const handleResetFilters = () => {
    setFilters({
      subject: '',
      level: '',
      location: '',
      minRate: rateBounds.min,
      maxRate: rateBounds.max,
      minRating: '',
    })
  }

  return (
    <main className="page-shell search-page-shell">
      <section className="card search-panel">
        <div className="search-header">
          <h1>Wyszukiwanie tutorów</h1>
          <p className="muted">Widok korzysta obecnie z mockowanych danych.</p>
        </div>
        <form className="search-filters" onSubmit={(event) => event.preventDefault()}>
          <label className="stack filter-field" htmlFor="subject">
            Przedmiot
            <select id="subject" name="subject" onChange={handleFilterChange} value={filters.subject}>
              <option value="">Wszystkie</option>
              {subjects.map((subject) => (
                <option key={subject} value={subject}>
                  {subject}
                </option>
              ))}
            </select>
          </label>

          <label className="stack filter-field" htmlFor="level">
            Poziom
            <select id="level" name="level" onChange={handleFilterChange} value={filters.level}>
              <option value="">Wszystkie</option>
              {levels.map((level) => (
                <option key={level} value={level}>
                  {level}
                </option>
              ))}
            </select>
          </label>

          <label className="stack filter-field" htmlFor="location">
            Lokalizacja
            <input
              id="location"
              name="location"
              onChange={handleFilterChange}
              placeholder="np. Warszawa"
              type="text"
              value={filters.location}
            />
          </label>

          <div className="stack filter-field rate-range">
            <div className="rate-range-header">
              <span>Stawka (PLN/h)</span>
              <strong>
                {filters.minRate} - {filters.maxRate}
              </strong>
            </div>
            <div className="rate-range-sliders" style={rateRangeStyle}>
              <input
                aria-label="Minimalna stawka"
                id="minRate"
                max={rateBounds.max}
                min={rateBounds.min}
                name="minRate"
                onChange={handleRateChange}
                type="range"
                value={filters.minRate}
              />
              <input
                aria-label="Maksymalna stawka"
                id="maxRate"
                max={rateBounds.max}
                min={rateBounds.min}
                name="maxRate"
                onChange={handleRateChange}
                type="range"
                value={filters.maxRate}
              />
            </div>
          </div>

          <label className="stack filter-field" htmlFor="minRating">
            Minimalna ocena
            <select
              id="minRating"
              name="minRating"
              onChange={handleFilterChange}
              value={filters.minRating}
            >
              <option value="">Dowolna</option>
              <option value="4.0">4.0+</option>
              <option value="4.5">4.5+</option>
              <option value="4.8">4.8+</option>
              <option value="5.0">5.0</option>
            </select>
          </label>

          <div className="search-filters-actions">
            <button
              aria-label="Wyczyść filtry"
              className="btn secondary icon-btn"
              onClick={handleResetFilters}
              title="Wyczyść filtry"
              type="button"
            >
              <svg aria-hidden="true" viewBox="0 0 24 24">
                <path
                  d="M9 3h6l1 2h4v2H4V5h4l1-2zm1 6h2v8h-2V9zm4 0h2v8h-2V9zM7 9h2v8H7V9zm-1 12h12l1-13H5l1 13z"
                  fill="currentColor"
                />
              </svg>
            </button>
          </div>
        </form>
        <p className="muted search-results-count">
          Znaleziono {filteredTutors.length} {filteredTutors.length === 1 ? 'tutora' : 'tutorów'}.
        </p>
      </section>

      <section className="tutor-grid">
        {filteredTutors.map((tutor) => (
          <article className="card tutor-card" key={tutor.id}>
            <h2>{tutor.name}</h2>
            <p>
              <strong>Przedmiot:</strong> {tutor.subject}
            </p>
            <p>
              <strong>Poziom:</strong> {tutor.level}
            </p>
            <p>
              <strong>Lokalizacja:</strong> {tutor.location}
            </p>
            <p>
              <strong>Stawka:</strong> {tutor.hourlyRate} PLN/h
            </p>
            <p>
              <strong>Ocena:</strong> {tutor.rating}
            </p>
            <button className="btn secondary" type="button">
              Zobacz profil
            </button>
          </article>
        ))}
        {!filteredTutors.length && (
          <article className="card">
            <h2>Brak wyników</h2>
            <p className="muted">Spróbuj zmienić kryteria filtrowania.</p>
          </article>
        )}
      </section>
    </main>
  )
}

export default SearchPage
