import { useEffect, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { fetchLessonRoomState } from '../api/lessonsApi'
import { createLessonChatConnection } from '../realtime/hubs'
import { useAuth } from '../context/AuthContext'

const LessonRoomPage = () => {
  const { lessonId } = useParams()
  const { user } = useAuth()
  const chatRef = useRef(null)
  const [messages, setMessages] = useState([])
  const [draft, setDraft] = useState('')
  const [hubStatus, setHubStatus] = useState('Łączenie...')
  const [sendError, setSendError] = useState('')

  useEffect(() => {
    const chat = createLessonChatConnection()
    let isDisposed = false
    chatRef.current = chat

    const receiveMessage = (payload) => {
      setMessages((prev) => (prev.some((m) => m.id === payload.id) ? prev : [...prev, payload]))
    }

    const start = async () => {
      try {
        const state = await fetchLessonRoomState(lessonId)
        if (isDisposed) {
          return
        }
        setMessages(Array.isArray(state?.messages) ? state.messages : [])
        chat.on('ReceiveMessage', receiveMessage)
        await chat.start()
        await chat.invoke('JoinLessonGroup', lessonId)
        if (!isDisposed) {
          setHubStatus('Połączono — czat aktywny')
        }
      } catch {
        if (!isDisposed) {
          setHubStatus('Nie udało się połączyć (sprawdź token i czy jesteś uczestnikiem lekcji).')
        }
      }
    }

    start()

    return () => {
      isDisposed = true
      chat.off('ReceiveMessage', receiveMessage)
      chat.stop()
      if (chatRef.current === chat) {
        chatRef.current = null
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

      <div className="lesson-room-grid lesson-room-grid--chat-only">
        <section className="card lesson-chat-panel">
          <h2>Czat lekcji</h2>
          <div className="chat-log">
            {messages.map((message) => (
              <div key={message.id} className="chat-line">
                <span className="muted">{new Date(message.sentAtUtc).toLocaleTimeString()}</span>{' '}
                <strong>{resolveSenderLabel(message.senderUserId)}</strong>: {message.message}
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
              onChange={(event) => setDraft(event.target.value)}
              placeholder="Wiadomość..."
              maxLength={4000}
            />
            <button className="btn primary" type="submit">
              Wyślij
            </button>
          </form>
        </section>

        <section className="card lesson-board-launch">
          <h2>Tablica</h2>
          <p className="muted">Tablica otwiera się w osobnej karcie, żeby było więcej miejsca na pracę.</p>
          <a className="btn primary" href={`/lessons/${lessonId}/whiteboard`} target="_blank" rel="noreferrer">
            Otwórz tablicę
          </a>
        </section>
      </div>
    </main>
  )
}

export default LessonRoomPage
