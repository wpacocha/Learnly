import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { fetchLessonRoomState } from '../api/lessonsApi'
import { createWhiteboardConnection } from '../realtime/hubs'

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

const LessonWhiteboardPage = () => {
  const { lessonId } = useParams()
  const canvasRef = useRef(null)
  const boardRef = useRef(null)
  const drawingRef = useRef(false)
  const lastPointRef = useRef(null)
  const boardEventsRef = useRef([])
  const toolRef = useRef('pen')
  const colorRef = useRef(BOARD_COLORS[1])

  const [tool, setTool] = useState('pen')
  const [color, setColor] = useState(BOARD_COLORS[1])
  const [status, setStatus] = useState('Łączenie...')
  const [error, setError] = useState('')

  useEffect(() => {
    toolRef.current = tool
  }, [tool])

  useEffect(() => {
    colorRef.current = color
  }, [color])

  const voiceRoomUrl = useMemo(
    () => `https://meet.jit.si/learnly-lesson-${String(lessonId || '').replace(/[^a-zA-Z0-9_-]/g, '')}`,
    [lessonId],
  )

  const clearCanvasLocal = () => {
    const canvas = canvasRef.current
    const ctx = canvas?.getContext('2d')
    if (!canvas || !ctx) {
      return
    }
    ctx.fillStyle = '#ffffff'
    ctx.fillRect(0, 0, canvas.width, canvas.height)
  }

  useEffect(() => {
    const canvas = canvasRef.current
    const ctx = canvas?.getContext('2d')
    if (!canvas || !ctx) {
      return undefined
    }

    const drawStroke = (p) => {
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
          drawStroke(JSON.parse(evt.payloadJson))
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
      const width = parent?.clientWidth ?? 1000
      canvas.width = width
      canvas.height = Math.max(520, Math.round(window.innerHeight * 0.68))
      replayBoardEvents()
    }

    resize()
    window.addEventListener('resize', resize)
    const resizeObserver = new ResizeObserver(resize)
    if (canvas.parentElement) {
      resizeObserver.observe(canvas.parentElement)
    }

    const board = createWhiteboardConnection()
    boardRef.current = board
    let isDisposed = false

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
        boardEventsRef.current = Array.isArray(state?.whiteboardEvents) ? state.whiteboardEvents : []
        replayBoardEvents()
        board.on('ReceiveEvent', receiveBoardEvent)
        await board.start()
        await board.invoke('JoinLessonGroup', lessonId)
        if (!isDisposed) {
          setStatus('Połączono — tablica aktywna')
        }
      } catch {
        if (!isDisposed) {
          setStatus('Nie udało się połączyć z tablicą.')
        }
      }
    }

    start()

    const pointerDown = (event) => {
      drawingRef.current = true
      const rect = canvas.getBoundingClientRect()
      lastPointRef.current = { x: event.clientX - rect.left, y: event.clientY - rect.top }
    }

    const pointerMove = (event) => {
      if (!drawingRef.current || !lastPointRef.current) {
        return
      }
      const rect = canvas.getBoundingClientRect()
      const x = event.clientX - rect.left
      const y = event.clientY - rect.top
      const prev = lastPointRef.current
      const erasing = toolRef.current === 'eraser'
      const payload = {
        x0: prev.x,
        y0: prev.y,
        x1: x,
        y1: y,
        color: erasing ? '#ffffff' : colorRef.current,
        lineWidth: erasing ? 18 : 2,
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
      board.off('ReceiveEvent', receiveBoardEvent)
      board.stop()
      if (boardRef.current === board) {
        boardRef.current = null
      }
    }
  }, [lessonId])

  const clearBoardForEveryone = async () => {
    setError('')
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
      setError('Nie udało się zsynchronizować czyszczenia tablicy.')
    }
  }

  const downloadBoardPdf = () => {
    setError('')
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
      setError('Nie udało się pobrać tablicy jako PDF.')
    }
  }

  return (
    <main className="page-shell lesson-whiteboard-page">
      <section className="lesson-whiteboard-header">
        <Link className="btn ghost" to={`/lessons/${lessonId}/room`}>
          ← Pokój lekcyjny
        </Link>
        <div>
          <h1>Tablica</h1>
          <p className="page-lead">{status}</p>
        </div>
        <a className="btn secondary" href={voiceRoomUrl} target="_blank" rel="noreferrer">
          Rozmowa głosowa
        </a>
      </section>

      <section className="lesson-whiteboard-surface">
        <div className="whiteboard-toolbar" aria-label="Narzędzia tablicy">
          <div className="whiteboard-tool-group" role="group" aria-label="Narzędzie">
            <button
              type="button"
              className={`btn ${tool === 'pen' ? 'primary' : 'secondary'} whiteboard-tool-btn`}
              onClick={() => setTool('pen')}
            >
              Pióro
            </button>
            <button
              type="button"
              className={`btn ${tool === 'eraser' ? 'primary' : 'secondary'} whiteboard-tool-btn`}
              onClick={() => setTool('eraser')}
            >
              Gumka
            </button>
          </div>
          <div className="whiteboard-color-group" role="group" aria-label="Kolor">
            {BOARD_COLORS.map((item) => (
              <button
                key={item}
                type="button"
                className={`whiteboard-color${color === item ? ' is-active' : ''}`}
                style={{ '--board-color': item }}
                onClick={() => {
                  setColor(item)
                  setTool('pen')
                }}
                aria-label={`Kolor ${item}`}
              />
            ))}
          </div>
          <button type="button" className="btn secondary" onClick={clearBoardForEveryone}>
            Wyczyść
          </button>
          <button type="button" className="btn secondary" onClick={downloadBoardPdf}>
            PDF
          </button>
        </div>
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
        <div className="canvas-wrap canvas-wrap--standalone">
          <canvas ref={canvasRef} className={`whiteboard-canvas whiteboard-canvas--${tool}`} />
        </div>
      </section>
    </main>
  )
}

export default LessonWhiteboardPage
