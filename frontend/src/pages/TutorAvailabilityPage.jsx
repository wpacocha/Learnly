import { useEffect, useMemo, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { fetchSubjects } from '../api/catalogApi'
import { getApiErrorMessage } from '../api/client'
import { fetchMyAssignments } from '../api/tutorAssignmentsApi'
import {
  createAvailabilitySlot,
  deleteAvailabilitySlot,
  fetchMyAvailabilitySlots,
} from '../api/tutorAvailabilityApi'

const pad = (value) => String(value).padStart(2, '0')

const toDateKey = (date) =>
  `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`

const formatWhen = (iso) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const formatDay = (dateKey) =>
  new Date(`${dateKey}T12:00`).toLocaleDateString(undefined, { dateStyle: 'medium' })

const teachingModeLabel = (m) => {
  if (m === 0) return 'Online'
  if (m === 1) return 'Stacjonarnie'
  return 'Online i stacjonarnie'
}

const monthLabel = (date) =>
  date.toLocaleDateString(undefined, { month: 'long', year: 'numeric' })

const buildMonthDays = (monthDate) => {
  const first = new Date(monthDate.getFullYear(), monthDate.getMonth(), 1)
  const firstWeekday = (first.getDay() + 6) % 7
  const daysInMonth = new Date(monthDate.getFullYear(), monthDate.getMonth() + 1, 0).getDate()
  const cells = []

  for (let i = 0; i < firstWeekday; i += 1) {
    cells.push(null)
  }

  for (let day = 1; day <= daysInMonth; day += 1) {
    cells.push(new Date(monthDate.getFullYear(), monthDate.getMonth(), day))
  }

  while (cells.length % 7 !== 0) {
    cells.push(null)
  }

  return cells
}

const rangesOverlap = (aStart, aEnd, bStart, bEnd) => aStart < bEnd && aEnd > bStart

const TutorAvailabilityPage = () => {
  const location = useLocation()
  const todayKey = toDateKey(new Date())
  const maxDate = useMemo(() => {
    const date = new Date()
    date.setMonth(date.getMonth() + 3)
    return date
  }, [])
  const maxDateKey = toDateKey(maxDate)

  const [slots, setSlots] = useState([])
  const [offerings, setOfferings] = useState([])
  const [subjects, setSubjects] = useState([])
  const [selectedOfferingId, setSelectedOfferingId] = useState('')
  const [selectedDateKey, setSelectedDateKey] = useState(todayKey)
  const [blockStartTime, setBlockStartTime] = useState('09:00')
  const [blockEndTime, setBlockEndTime] = useState('12:00')
  const [monthOffset, setMonthOffset] = useState(0)
  const [draftSlots, setDraftSlots] = useState([])
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

  const currentMonth = useMemo(() => {
    const date = new Date()
    date.setDate(1)
    date.setMonth(date.getMonth() + monthOffset)
    return date
  }, [monthOffset])

  const monthDays = useMemo(() => buildMonthDays(currentMonth), [currentMonth])

  const selectedDraftSlots = useMemo(
    () => draftSlots.filter((slot) => slot.dateKey === selectedDateKey),
    [draftSlots, selectedDateKey],
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
      } catch (err) {
        setError(getApiErrorMessage(err, 'Nie udało się pobrać danych. Upewnij się, że masz profil tutora i zapisaną ofertę.'))
      } finally {
        setIsLoading(false)
      }
    }
    load()
  }, [location.key, location.state?.preselectOfferingId])

  const addDraftBlock = (event) => {
    event.preventDefault()
    setError('')
    setMessage('')

    if (!selectedOffering) {
      setError('Wybierz pozycję oferty.')
      return
    }

    if (!selectedDateKey || selectedDateKey < todayKey || selectedDateKey > maxDateKey) {
      setError('Wybierz datę od dzisiaj do maksymalnie 3 miesięcy do przodu.')
      return
    }

    const blockStart = new Date(`${selectedDateKey}T${blockStartTime}`)
    const blockEnd = new Date(`${selectedDateKey}T${blockEndTime}`)
    const now = new Date()
    const durationMs = selectedOffering.durationMinutes * 60 * 1000

    if (Number.isNaN(blockStart.getTime()) || Number.isNaN(blockEnd.getTime()) || blockEnd <= blockStart) {
      setError('Wybierz poprawny zakres godzin.')
      return
    }

    if (blockStart < now) {
      setError('Nie można dodać dostępności wcześniejszej niż aktualna data i godzina.')
      return
    }

    const created = []
    for (let cursor = blockStart.getTime(); cursor + durationMs <= blockEnd.getTime(); cursor += durationMs) {
      const start = new Date(cursor)
      const end = new Date(cursor + durationMs)
      const overlapsSaved = slots.some((slot) =>
        rangesOverlap(start, end, new Date(slot.startUtc), new Date(slot.endUtc)),
      )
      const overlapsDraft = draftSlots.some((slot) =>
        rangesOverlap(start, end, new Date(slot.startUtc), new Date(slot.endUtc)),
      )

      if (!overlapsSaved && !overlapsDraft) {
        created.push({
          clientId: crypto.randomUUID(),
          tutorTeachingOfferingId: selectedOffering.id,
          subjectId: selectedOffering.subjectId,
          dateKey: selectedDateKey,
          startUtc: start.toISOString(),
          endUtc: end.toISOString(),
        })
      }
    }

    if (created.length === 0) {
      setError('Wybrany blok nie mieści żadnej nowej lekcji albo nakłada się na istniejące sloty.')
      return
    }

    setDraftSlots((prev) => [...prev, ...created])
    setMessage(`Dodano do szkicu ${created.length} slotów. Kliknij „Zapisz”, aby zapisać je w bazie.`)
  }

  const removeDraftSlot = (clientId) => {
    setDraftSlots((prev) => prev.filter((slot) => slot.clientId !== clientId))
  }

  const saveDraftSlots = async () => {
    setError('')
    setMessage('')
    if (draftSlots.length === 0) {
      setError('Brak nowych slotów do zapisania.')
      return
    }

    setIsSaving(true)
    try {
      for (const slot of draftSlots) {
        await createAvailabilitySlot({
          tutorTeachingOfferingId: slot.tutorTeachingOfferingId,
          startUtc: slot.startUtc,
        })
      }
      setDraftSlots([])
      setMessage('Dostępność została zapisana.')
      await reload()
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } catch (err) {
      setError(getApiErrorMessage(err, 'Nie udało się zapisać dostępności.'))
      await reload()
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
    } catch (err) {
      setError(getApiErrorMessage(err, 'Usuwanie nie powiodło się.'))
    } finally {
      setDeletingId(null)
    }
  }

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered loading-state">
          <p>Ładowanie dostępności...</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <section className="card">
        <h1>Twoja dostępność</h1>
        <p className="page-lead">
          Wybierz pozycję oferty, zaznacz dzień na kartce miesiąca i dodaj blok godzin. Blok zostanie podzielony na lekcje
          według długości ustawionej w{' '}
          <Link className="text-link" to="/tutor/profile">
            profilu tutora
          </Link>
          . Zapis do bazy następuje dopiero po kliknięciu przycisku „Zapisz”.
        </p>

        {offerings.length === 0 && (
          <p className="error" role="alert">
            Nie masz zapisanej oferty. Dodaj przedmioty w profilu tutora, potem wróć tutaj.
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

        <div className="availability-planner">
          <div className="availability-planner__controls">
            <label htmlFor="offeringPick">Pozycja oferty</label>
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

            <form className="form availability-block-form" onSubmit={addDraftBlock}>
              <div className="availability-block-form__row">
                <label htmlFor="blockStart">Od</label>
                <input
                  id="blockStart"
                  type="time"
                  value={blockStartTime}
                  step={300}
                  onChange={(e) => setBlockStartTime(e.target.value)}
                  disabled={offerings.length === 0}
                  required
                />
                <label htmlFor="blockEnd">Do</label>
                <input
                  id="blockEnd"
                  type="time"
                  value={blockEndTime}
                  step={300}
                  onChange={(e) => setBlockEndTime(e.target.value)}
                  disabled={offerings.length === 0}
                  required
                />
              </div>
              <button className="btn secondary" type="submit" disabled={offerings.length === 0}>
                Dodaj blok do szkicu
              </button>
            </form>

            <div className="availability-draft-actions">
              <button className="btn primary" type="button" onClick={saveDraftSlots} disabled={isSaving || draftSlots.length === 0}>
                {isSaving ? 'Zapisywanie...' : `Zapisz (${draftSlots.length})`}
              </button>
              <button className="btn ghost" type="button" onClick={() => setDraftSlots([])} disabled={isSaving || draftSlots.length === 0}>
                Wyczyść szkic
              </button>
            </div>
          </div>

          <div className="availability-calendar">
            <div className="availability-calendar__toolbar">
              <button className="btn ghost" type="button" onClick={() => setMonthOffset((v) => Math.max(0, v - 1))} disabled={monthOffset === 0}>
                Poprzedni
              </button>
              <h2>{monthLabel(currentMonth)}</h2>
              <button className="btn ghost" type="button" onClick={() => setMonthOffset((v) => Math.min(3, v + 1))} disabled={monthOffset === 3}>
                Następny
              </button>
            </div>
            <div className="availability-calendar__weekdays" aria-hidden="true">
              {['Pon', 'Wt', 'Śr', 'Czw', 'Pt', 'Sob', 'Nd'].map((day) => (
                <span key={day}>{day}</span>
              ))}
            </div>
            <div className="availability-calendar__grid">
              {monthDays.map((date, index) => {
                if (!date) {
                  return <span key={`empty-${index}`} className="availability-calendar__empty" />
                }
                const dateKey = toDateKey(date)
                const disabled = dateKey < todayKey || dateKey > maxDateKey
                const savedCount = slots.filter((slot) => toDateKey(new Date(slot.startUtc)) === dateKey).length
                const draftCount = draftSlots.filter((slot) => slot.dateKey === dateKey).length
                return (
                  <button
                    key={dateKey}
                    type="button"
                    className={`availability-day${selectedDateKey === dateKey ? ' availability-day--selected' : ''}`}
                    disabled={disabled}
                    onClick={() => setSelectedDateKey(dateKey)}
                  >
                    <span>{date.getDate()}</span>
                    {(savedCount > 0 || draftCount > 0) && (
                      <small>
                        {savedCount > 0 ? `${savedCount} zapis.` : ''}
                        {draftCount > 0 ? ` ${draftCount} szkic` : ''}
                      </small>
                    )}
                  </button>
                )
              })}
            </div>
          </div>
        </div>
      </section>

      <section className="card">
        <h2>Szkic dla dnia: {formatDay(selectedDateKey)}</h2>
        {selectedDraftSlots.length === 0 && <p className="muted">Brak nowych slotów dla wybranego dnia.</p>}
        <ul className="slot-list">
          {selectedDraftSlots.map((slot) => (
            <li key={slot.clientId} className="slot-row">
              <span>
                {subjectById.get(slot.subjectId) ? `${subjectById.get(slot.subjectId)} · ` : ''}
                {formatWhen(slot.startUtc)} — {formatWhen(slot.endUtc)}
              </span>
              <button type="button" className="btn secondary" onClick={() => removeDraftSlot(slot.clientId)}>
                Usuń ze szkicu
              </button>
            </li>
          ))}
        </ul>
      </section>

      <section className="card">
        <h2>Zapisane sloty</h2>
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
