import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { createLessonChatConnection, createWhiteboardConnection } from '../realtime/hubs'
import { useAuth } from '../context/AuthContext'

const LessonRoomPage = () => {
  const { lessonId } = useParams()
  const { user } = useAuth()
  const canvasRef = useRef(null)
  const chatRef = useRef(null)
  const boardRef = useRef(null)
  const drawingRef = useRef(false)
  const lastPointRef = useRef(null)

  const [messages, setMessages] = useState([])
  const [draft, setDraft] = useState('')
  const [hubStatus, setHubStatus] = useState('Łączenie...')
  const [sendError, setSendError] = useState('')
  const [boardError, setBoardError] = useState('')
  const [isVoiceOpen, setIsVoiceOpen] = useState(false)

  const voiceRoomUrl = useMemo(
    () => `https://meet.jit.si/learnly-lesson-${String(lessonId || '').replace(/[^a-zA-Z0-9_-]/g, '')}`,
    [lessonId],
  )

  const clearCanvasLocal = () => {
    const canvas = canvasRef.current
    if (!canvas) {
      return
    }
    const ctx = canvas.getContext('2d')
    if (!ctx) {
      return
    }
    ctx.fillStyle = '#ffffff'
    ctx.fillRect(0, 0, canvas.width, canvas.height)
  }

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) {
      return undefined
    }
    const ctx = canvas.getContext('2d')
    const resize = () => {
      const parent = canvas.parentElement
      const w = parent?.clientWidth ?? 800
      canvas.width = w
      canvas.height = 420
      clearCanvasLocal()
    }
    resize()
    window.addEventListener('resize', resize)

    const drawStroke = (p) => {
      if (!ctx) {
        return
      }
      ctx.strokeStyle = p.color || '#111827'
      ctx.lineWidth = p.lineWidth || 2
      ctx.lineCap = 'round'
      ctx.beginPath()
      ctx.moveTo(p.x0, p.y0)
      ctx.lineTo(p.x1, p.y1)
      ctx.stroke()
    }

    const chat = createLessonChatConnection()
    const board = createWhiteboardConnection()
    chatRef.current = chat
    boardRef.current = board

    const start = async () => {
      try {
        await chat.start()
        await chat.invoke('JoinLessonGroup', lessonId)
        chat.on('ReceiveMessage', (payload) => {
          setMessages((prev) => [...prev, payload])
        })

        await board.start()
        await board.invoke('JoinLessonGroup', lessonId)
        board.on('ReceiveEvent', (evt) => {
          if (evt.eventType === 'stroke') {
            try {
              const p = JSON.parse(evt.payloadJson)
              drawStroke(p)
            } catch {
              /* ignore malformed payload */
            }
          } else if (evt.eventType === 'clear') {
            clearCanvasLocal()
          }
        })

        setHubStatus('Połączono — czat i tablica aktywne')
      } catch {
        setHubStatus('Nie udało się połączyć (sprawdź token i czy jesteś uczestnikiem lekcji).')
      }
    }

    start()

    const pointerDown = (e) => {
      drawingRef.current = true
      const r = canvas.getBoundingClientRect()
      lastPointRef.current = { x: e.clientX - r.left, y: e.clientY - r.top }
    }

    const pointerMove = (e) => {
      if (!drawingRef.current || !lastPointRef.current) {
        return
      }
      const r = canvas.getBoundingClientRect()
      const x = e.clientX - r.left
      const y = e.clientY - r.top
      const prev = lastPointRef.current
      const payload = {
        x0: prev.x,
        y0: prev.y,
        x1: x,
        y1: y,
        color: '#4338ca',
        lineWidth: 2,
      }
      drawStroke(payload)
      lastPointRef.current = { x, y }
      board
        .invoke('SendEvent', {
          lessonId,
          eventType: 'stroke',
          payloadJson: JSON.stringify(payload),
        })
        .catch(() => {})
    }

    const pointerUp = () => {
      drawingRef.current = false
      lastPointRef.current = null
    }

    canvas.addEventListener('mousedown', pointerDown)
    canvas.addEventListener('mousemove', pointerMove)
    window.addEventListener('mouseup', pointerUp)

    return () => {
      window.removeEventListener('resize', resize)
      canvas.removeEventListener('mousedown', pointerDown)
      canvas.removeEventListener('mousemove', pointerMove)
      window.removeEventListener('mouseup', pointerUp)
      chat.stop()
      board.stop()
      chatRef.current = null
      boardRef.current = null
    }
  }, [lessonId])

  const sendChat = async (event) => {
    event.preventDefault()
    const text = draft.trim()
    if (!text) {
      return
    }
    setSendError('')
    try {
      const chat = chatRef.current
      if (!chat) {
        return
      }
      await chat.invoke('SendMessage', { lessonId, message: text })
      setDraft('')
    } catch {
      setSendError('Nie udało się wysłać wiadomości.')
    }
  }

  const clearBoardForEveryone = async () => {
    setBoardError('')
    clearCanvasLocal()
    try {
      const board = boardRef.current
      if (!board) {
        return
      }
      await board.invoke('SendEvent', {
        lessonId,
        eventType: 'clear',
        payloadJson: '{}',
      })
    } catch {
      setBoardError('Nie udało się zsynchronizować czyszczenia tablicy.')
    }
  }

  const resolveSenderLabel = (senderUserId) => {
    if (!senderUserId) {
      return 'Uczestnik'
    }
    if (senderUserId === user?.id) {
      return 'Ty'
    }
    if (user?.roles?.includes('Tutor')) {
      return 'Student'
    }
    if (user?.roles?.includes('Student')) {
      return 'Tutor'
    }
    return 'Uczestnik'
  }

  return (
    <main className="page-shell lesson-room">
      <section className="card lesson-room-header">
        <Link className="btn ghost" to="/lessons">
          ← Moje lekcje
        </Link>
        <h1>Pokój lekcyjny</h1>
        <p className="page-lead">{hubStatus}</p>
      </section>

      <div className="lesson-room-grid">
        <section className="card lesson-chat-panel">
          <h2>Czat</h2>
          <div className="chat-log">
            {messages.map((m) => (
              <div key={m.id} className="chat-line">
                <span className="muted">{new Date(m.sentAtUtc).toLocaleTimeString()}</span>{' '}
                <strong>{resolveSenderLabel(m.senderUserId)}</strong>: {m.message}
              </div>
            ))}
            {!messages.length && <p className="muted">Brak wiadomości — napisz coś poniżej.</p>}
          </div>
          {sendError && (
            <p className="error" role="alert">
              {sendError}
            </p>
          )}
          <form className="chat-compose" onSubmit={sendChat}>
            <input
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              placeholder="Wiadomość…"
              maxLength={4000}
            />
            <button className="btn primary" type="submit">
              Wyślij
            </button>
          </form>
        </section>

        <section className="card lesson-board-panel">
          <h2>Tablica</h2>
          <p className="muted">Rysuj myszą — druga osoba widzi to na żywo.</p>
          <div className="lesson-board-actions">
            <button type="button" className="btn secondary" onClick={clearBoardForEveryone}>
              Wyczyść tablicę
            </button>
            <button type="button" className="btn primary" onClick={() => setIsVoiceOpen((v) => !v)}>
              {isVoiceOpen ? 'Ukryj rozmowę głosową' : 'Rozmowa głosowa'}
            </button>
            <a className="btn ghost" href={voiceRoomUrl} target="_blank" rel="noreferrer">
              Otwórz w nowej karcie
            </a>
          </div>
          {boardError && (
            <p className="error" role="alert">
              {boardError}
            </p>
          )}
          <div className="canvas-wrap">
            <canvas ref={canvasRef} className="whiteboard-canvas" />
          </div>
          {isVoiceOpen && (
            <div className="lesson-voice-wrap">
              <iframe
                title="Rozmowa głosowa"
                src={voiceRoomUrl}
                allow="camera; microphone; fullscreen; display-capture"
              />
            </div>
          )}
        </section>
      </div>
    </main>
  )
}

export default LessonRoomPage
