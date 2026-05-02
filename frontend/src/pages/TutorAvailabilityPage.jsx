import { useEffect, useMemo, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { fetchMyAssignments } from '../api/tutorAssignmentsApi'
import {
  createAvailabilitySlot,
  deleteAvailabilitySlot,
  fetchMyAvailabilitySlots,
} from '../api/tutorAvailabilityApi'
import { fetchSubjects } from '../api/catalogApi'

const formatWhen = (iso) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const teachingModeLabel = (m) => {
  if (m === 0) return 'Online'
  if (m === 1) return 'Stacjonarnie'
  return 'Online i stacjonarnie'
}

const TutorAvailabilityPage = () => {
  const location = useLocation()
  const [slots, setSlots] = useState([])
  const [offerings, setOfferings] = useState([])
  const [subjects, setSubjects] = useState([])
  const [selectedOfferingId, setSelectedOfferingId] = useState('')
  const [startLocal, setStartLocal] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [deletingId, setDeletingId] = useState(null)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const subjectById = useMemo(() => new Map(subjects.map((s) => [s.id, s.name])), [subjects])

  const selectedOffering = useMemo(
    () => offerings.find((o) => o.id === selectedOfferingId),
    [offerings, selectedOfferingId],
  )

  const reload = async () => {
    const data = await fetchMyAvailabilitySlots()
    setSlots(Array.isArray(data) ? data : [])
  }

  useEffect(() => {
    const load = async () => {
      setIsLoading(true)
      setError('')
      try {
        const [list, catalog, slotData] = await Promise.all([
          fetchMyAssignments(),
          fetchSubjects(),
          fetchMyAvailabilitySlots(),
        ])
        setSubjects(Array.isArray(catalog) ? catalog : [])
        setSlots(Array.isArray(slotData) ? slotData : [])
        if (Array.isArray(list) && list.length > 0) {
          setOfferings(list)
          const preId = location.state?.preselectOfferingId
          const pick =
            preId && list.some((o) => o.id === preId) ? preId : list[0].id
          setSelectedOfferingId(pick)
        } else {
          setOfferings([])
          setSelectedOfferingId('')
        }
      } catch {
        setError('Nie udało się pobrać danych. Upewnij się, że masz profil tutora i zapisaną ofertę w profilu.')
      } finally {
        setIsLoading(false)
      }
    }
    load()
  }, [location.key, location.state?.preselectOfferingId])

  const endPreviewLabel = useMemo(() => {
    if (!startLocal || !selectedOffering?.durationMinutes) {
      return null
    }
    const start = new Date(startLocal)
    if (Number.isNaN(start.getTime())) {
      return null
    }
    const end = new Date(start.getTime() + selectedOffering.durationMinutes * 60 * 1000)
    return formatWhen(end.toISOString())
  }, [startLocal, selectedOffering])

  const handleAdd = async (event) => {
    event.preventDefault()
    setError('')
    setMessage('')
    if (!selectedOfferingId) {
      setError('Wybierz przedmiot (pozycję oferty z profilu).')
      return
    }
    if (!startLocal) {
      setError('Wybierz początek slotu.')
      return
    }
    const start = new Date(startLocal)
    if (Number.isNaN(start.getTime())) {
      setError('Niepoprawna data początku.')
      return
    }
    const startUtc = start.toISOString()
    setIsSaving(true)
    try {
      await createAvailabilitySlot({
        tutorTeachingOfferingId: selectedOfferingId,
        startUtc,
      })
      setMessage('Dodano termin.')
      setStartLocal('')
      await reload()
    } catch (err) {
      const msg = err?.response?.data?.message
      setError(msg || 'Nie udało się dodać terminu.')
    } finally {
      setIsSaving(false)
    }
  }

  const handleDelete = async (id) => {
    setDeletingId(id)
    setError('')
    try {
      await deleteAvailabilitySlot(id)
      await reload()
    } catch {
      setError('Usuwanie nie powiodło się.')
    } finally {
      setDeletingId(null)
    }
  }

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered loading-state">
          <p>Ładowanie dostępności…</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <section className="card">
        <h1>Twoja dostępność</h1>
        <p className="page-lead">
          Długość lekcji bierzemy z pozycji oferty w{' '}
          <Link className="text-link" to="/tutor/profile">
            profilu tutora
          </Link>
          . Wybierz przedmiot (ofertę) i <strong>start</strong> slotu — koniec wyliczamy automatycznie. Czas w Twojej
          strefie, zapis w UTC.
        </p>
        {offerings.length === 0 && (
          <p className="error" role="alert">
            Nie masz zapisanej oferty (przedmiotów). Dodaj je w profilu tutora, potem wróć tutaj.
          </p>
        )}
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
        {message && (
          <p className="success" role="status">
            {message}
          </p>
        )}
        <form className="form availability-form" onSubmit={handleAdd}>
          <label htmlFor="offeringPick">Pozycja oferty (przedmiot)</label>
          <select
            id="offeringPick"
            value={selectedOfferingId}
            onChange={(e) => setSelectedOfferingId(e.target.value)}
            required
            disabled={offerings.length === 0}
          >
            {offerings.length === 0 ? (
              <option value="">— brak oferty —</option>
            ) : (
              offerings.map((o) => (
                <option key={o.id} value={o.id}>
                  {subjectById.get(o.subjectId) ?? `Przedmiot #${o.subjectId}`} · {teachingModeLabel(o.teachingMode)} ·{' '}
                  {o.durationMinutes} min
                </option>
              ))
            )}
          </select>

          <label htmlFor="slotStart">Początek slotu</label>
          <input
            id="slotStart"
            type="datetime-local"
            value={startLocal}
            onChange={(e) => setStartLocal(e.target.value)}
            required
            disabled={offerings.length === 0}
          />

          {endPreviewLabel && (
            <p className="muted availability-end-preview" role="status">
              Koniec slotu: <strong>{endPreviewLabel}</strong>
              {selectedOffering ? ` (${selectedOffering.durationMinutes} min wg profilu)` : ''}
            </p>
          )}

          <button className="btn primary" type="submit" disabled={isSaving || offerings.length === 0}>
            {isSaving ? 'Dodawanie...' : 'Dodaj slot'}
          </button>
        </form>
      </section>

      <section className="card">
        <h2>Lista slotów</h2>
        {slots.length === 0 && <p className="muted">Brak zdefiniowanej dostępności.</p>}
        <ul className="slot-list">
          {slots.map((s) => (
            <li key={s.id} className="slot-row">
              <span>
                {subjectById.get(s.subjectId) ? `${subjectById.get(s.subjectId)} · ` : ''}
                {formatWhen(s.startUtc)} — {formatWhen(s.endUtc)}
              </span>
              <button
                type="button"
                className="btn secondary"
                disabled={deletingId === s.id}
                onClick={() => handleDelete(s.id)}
              >
                {deletingId === s.id ? 'Usuwanie...' : 'Usuń'}
              </button>
            </li>
          ))}
        </ul>
      </section>
    </main>
  )
}

export default TutorAvailabilityPage
