import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { fetchLessonRoomState } from '../api/lessonsApi'
import { createLessonChatConnection, createWhiteboardConnection } from '../realtime/hubs'
import { useAuth } from '../context/AuthContext'

const BOARD_COLORS = ['#111827', '#4338ca', '#dc2626', '#16a34a', '#f59e0b']

const createPdfFromCanvas = (canvas) => {
  const imageData = canvas.toDataURL('image/jpeg', 0.95).split(',')[1]
  const imageBytes = atob(imageData)
  const pageWidth = 842
  const pageHeight = 595
  const margin = 32
  const maxImageWidth = pageWidth - margin * 2
  const maxImageHeight = pageHeight - margin * 2
  const scale = Math.min(maxImageWidth / canvas.width, maxImageHeight / canvas.height)
  const imageWidth = canvas.width * scale
  const imageHeight = canvas.height * scale
  const imageX = (pageWidth - imageWidth) / 2
  const imageY = (pageHeight - imageHeight) / 2
  const content = `q ${imageWidth.toFixed(2)} 0 0 ${imageHeight.toFixed(2)} ${imageX.toFixed(2)} ${imageY.toFixed(2)} cm /Im0 Do Q`
  const objects = [
    '<< /Type /Catalog /Pages 2 0 R >>',
    '<< /Type /Pages /Kids [3 0 R] /Count 1 >>',
    `<< /Type /Page /Parent 2 0 R /MediaBox [0 0 ${pageWidth} ${pageHeight}] /Resources << /XObject << /Im0 4 0 R >> >> /Contents 5 0 R >>`,
    `<< /Type /XObject /Subtype /Image /Width ${canvas.width} /Height ${canvas.height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length ${imageBytes.length} >>\nstream\n${imageBytes}\nendstream`,
    `<< /Length ${content.length} >>\nstream\n${content}\nendstream`,
  ]
  let pdf = '%PDF-1.4\n'
  const offsets = [0]
  objects.forEach((object, index) => {
    offsets.push(pdf.length)
    pdf += `${index + 1} 0 obj\n${object}\nendobj\n`
  })
  const xrefOffset = pdf.length
  pdf += `xref\n0 ${objects.length + 1}\n0000000000 65535 f \n`
  offsets.slice(1).forEach((offset) => {
    pdf += `${String(offset).padStart(10, '0')} 00000 n \n`
  })
  pdf += `trailer\n<< /Size ${objects.length + 1} /Root 1 0 R >>\nstartxref\n${xrefOffset}\n%%EOF`
  return new Blob([Uint8Array.from(pdf, (char) => char.charCodeAt(0))], { type: 'application/pdf' })
}

const LessonRoomPage = () => {
  const { lessonId } = useParams()
  const { user } = useAuth()
  const canvasRef = useRef(null)
  const chatRef = useRef(null)
  const boardRef = useRef(null)
  const drawingRef = useRef(false)
  const lastPointRef = useRef(null)
  const boardEventsRef = useRef([])
  const boardToolRef = useRef('draw')
  const boardColorRef = useRef(BOARD_COLORS[1])

  const [messages, setMessages] = useState([])
  const [draft, setDraft] = useState('')
  const [hubStatus, setHubStatus] = useState('Łączenie...')
  const [sendError, setSendError] = useState('')
  const [boardError, setBoardError] = useState('')
  const [isVoiceOpen, setIsVoiceOpen] = useState(false)
  const [boardTool, setBoardTool] = useState('draw')
  const [boardColor, setBoardColor] = useState(BOARD_COLORS[1])
  const [isBoardExpanded, setIsBoardExpanded] = useState(false)

  useEffect(() => {
    boardToolRef.current = boardTool
  }, [boardTool])

  useEffect(() => {
    boardColorRef.current = boardColor
  }, [boardColor])

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

    const applyBoardEvent = (evt) => {
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
    }

    const replayBoardEvents = () => {
      clearCanvasLocal()
      boardEventsRef.current.forEach(applyBoardEvent)
    }

    const resize = () => {
      const parent = canvas.parentElement
      const w = parent?.clientWidth ?? 800
      canvas.width = w
      canvas.height = 420
      replayBoardEvents()
    }
    resize()
    window.addEventListener('resize', resize)
    const resizeObserver = new ResizeObserver(resize)
    if (canvas.parentElement) {
      resizeObserver.observe(canvas.parentElement)
    }

    const chat = createLessonChatConnection()
    const board = createWhiteboardConnection()
    let isDisposed = false
    chatRef.current = chat
    boardRef.current = board

    const receiveMessage = (payload) => {
      setMessages((prev) => (prev.some((m) => m.id === payload.id) ? prev : [...prev, payload]))
    }

    const receiveBoardEvent = (evt) => {
      if (boardEventsRef.current.some((e) => e.id === evt.id)) {
        return
      }
      boardEventsRef.current = [...boardEventsRef.current, evt]
      applyBoardEvent(evt)
    }

    const start = async () => {
      try {
        const state = await fetchLessonRoomState(lessonId)
        if (isDisposed) {
          return
        }
        const initialMessages = Array.isArray(state?.messages) ? state.messages : []
        const initialBoardEvents = Array.isArray(state?.whiteboardEvents) ? state.whiteboardEvents : []
        setMessages(initialMessages)
        boardEventsRef.current = initialBoardEvents
        replayBoardEvents()

        chat.on('ReceiveMessage', receiveMessage)
        await chat.start()
        if (isDisposed) {
          await chat.stop()
          return
        }
        await chat.invoke('JoinLessonGroup', lessonId)
        if (isDisposed) {
          await chat.stop()
          return
        }

        board.on('ReceiveEvent', receiveBoardEvent)
        await board.start()
        if (isDisposed) {
          await board.stop()
          return
        }
        await board.invoke('JoinLessonGroup', lessonId)
        if (isDisposed) {
          await board.stop()
          return
        }

        setHubStatus('Połączono — czat i tablica aktywne')
      } catch {
        if (isDisposed) {
          return
        }
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
        color: boardColorRef.current,
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
      isDisposed = true
      window.removeEventListener('resize', resize)
      resizeObserver.disconnect()
      canvas.removeEventListener('mousedown', pointerDown)
      canvas.removeEventListener('mousemove', pointerMove)
      window.removeEventListener('mouseup', pointerUp)
      chat.off('ReceiveMessage', receiveMessage)
      board.off('ReceiveEvent', receiveBoardEvent)
      chat.stop()
      board.stop()
      if (chatRef.current === chat) {
        chatRef.current = null
      }
      if (boardRef.current === board) {
        boardRef.current = null
      }
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

  const downloadBoardPdf = () => {
    setBoardError('')
    try {
      const canvas = canvasRef.current
      if (!canvas) {
        return
      }
      const blob = createPdfFromCanvas(canvas)
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `tablica-lekcja-${lessonId}.pdf`
      document.body.appendChild(link)
      link.click()
      link.remove()
      window.setTimeout(() => URL.revokeObjectURL(url), 1000)
    } catch {
      setBoardError('Nie udało się pobrać tablicy jako PDF.')
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

        <section className={`card lesson-board-panel${isBoardExpanded ? ' lesson-board-panel--expanded' : ''}`}>
          <h2>Tablica</h2>
          <p className="muted">Rysuj myszą — druga osoba widzi to na żywo.</p>
          <div className="whiteboard-toolbar" aria-label="Narzedzia tablicy">
            <div className="whiteboard-tool-group" role="group" aria-label="Tryb tablicy">
              <button
                type="button"
                className={`btn ${boardTool === 'draw' ? 'primary' : 'secondary'} whiteboard-tool-btn`}
                onClick={() => setBoardTool('draw')}
              >
                Rysuj
              </button>
            </div>
            <div className="whiteboard-color-group" role="group" aria-label="Kolor">
              {BOARD_COLORS.map((color) => (
                <button
                  key={color}
                  type="button"
                  className={`whiteboard-color${boardColor === color ? ' is-active' : ''}`}
                  style={{ '--board-color': color }}
                  onClick={() => setBoardColor(color)}
                  aria-label={`Kolor ${color}`}
                />
              ))}
            </div>
          </div>
          <div className="lesson-board-actions">
            <button type="button" className="btn secondary" onClick={clearBoardForEveryone}>
              Wyczyść tablicę
            </button>
            <button type="button" className="btn secondary" onClick={downloadBoardPdf}>
              Pobierz PDF
            </button>
            <button type="button" className="btn secondary" onClick={() => setIsBoardExpanded((v) => !v)}>
              {isBoardExpanded ? 'Zmniejsz tablice' : 'Otworz tablice'}
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
            <canvas ref={canvasRef} className={`whiteboard-canvas whiteboard-canvas--${boardTool}`} />
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
