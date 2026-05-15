import { useEffect, useMemo, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { getApiErrorMessage } from '../api/client'
import {
  fetchConversationMessages,
  fetchConversations,
  sendConversationMessage,
  startConversation,
} from '../api/conversationsApi'
import { useAuth } from '../context/AuthContext'
import { useLanguage } from '../context/LanguageContext'

const formatWhen = (iso) =>
  new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })

const ConversationsPage = () => {
  const location = useLocation()
  const { user } = useAuth()
  const { t } = useLanguage()
  const [conversations, setConversations] = useState([])
  const [selectedId, setSelectedId] = useState('')
  const [messages, setMessages] = useState([])
  const [draft, setDraft] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isSending, setIsSending] = useState(false)
  const [error, setError] = useState('')

  const currentUserId = String(user?.id ?? user?.userId ?? '')
  const selectedConversation = useMemo(
    () => conversations.find((conversation) => conversation.id === selectedId),
    [conversations, selectedId],
  )

  const isOwnMessage = (message) => String(message.senderUserId) === currentUserId
  const isStudentSide = (conversation) => String(conversation?.studentUserId ?? '') === currentUserId
  const conversationTitle = (conversation) =>
    isStudentSide(conversation) ? conversation.tutorName : conversation.studentEmail
  const conversationSubtitle = (conversation) =>
    isStudentSide(conversation) ? conversation.studentEmail : conversation.tutorName

  useEffect(() => {
    let cancelled = false
    const load = async () => {
      setIsLoading(true)
      setError('')
      try {
        let preselected = null
        if (location.state?.tutorProfileId) {
          preselected = await startConversation(location.state.tutorProfileId)
        }
        const list = await fetchConversations()
        if (cancelled) {
          return
        }
        setConversations(Array.isArray(list) ? list : [])
        const pick = preselected?.id ?? list?.[0]?.id ?? ''
        setSelectedId(pick)
      } catch (err) {
        if (!cancelled) {
          setError(getApiErrorMessage(err, 'Nie udalo sie pobrac rozmow.'))
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false)
        }
      }
    }
    load()
    return () => {
      cancelled = true
    }
  }, [location.key, location.state?.tutorProfileId])

  useEffect(() => {
    let cancelled = false
    const loadMessages = async () => {
      if (!selectedId) {
        setMessages([])
        return
      }
      try {
        const data = await fetchConversationMessages(selectedId)
        if (!cancelled) {
          setMessages(Array.isArray(data) ? data : [])
        }
      } catch (err) {
        if (!cancelled) {
          setError(getApiErrorMessage(err, 'Nie udalo sie pobrac wiadomosci.'))
        }
      }
    }
    loadMessages()
    const id = window.setInterval(loadMessages, 10000)
    return () => {
      cancelled = true
      window.clearInterval(id)
    }
  }, [selectedId])

  const submitMessage = async (event) => {
    event.preventDefault()
    if (!selectedId || !draft.trim()) {
      return
    }
    setIsSending(true)
    setError('')
    try {
      const saved = await sendConversationMessage(selectedId, draft.trim())
      setMessages((prev) => [...prev, saved])
      setDraft('')
      const list = await fetchConversations()
      setConversations(Array.isArray(list) ? list : [])
    } catch (err) {
      setError(getApiErrorMessage(err, 'Nie udalo sie wyslac wiadomosci.'))
    } finally {
      setIsSending(false)
    }
  }

  if (isLoading) {
    return (
      <main className="page-shell">
        <section className="card centered loading-state">
          <p>{t('Ladowanie rozmow...')}</p>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <section className="card">
        <h1>{t('Chat z tutorami')}</h1>
        <p className="page-lead">{t('Pisz z prowadzacym niezaleznie od ustalonej lekcji.')}</p>
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
        {conversations.length === 0 ? (
          <p className="muted">{t('Brak rozmow. Wejdz w profil tutora i kliknij "Napisz do tutora".')}</p>
        ) : (
          <div className="conversation-layout">
            <aside className="conversation-list" aria-label={t('Rozmowy')}>
              {conversations.map((conversation) => (
                <button
                  key={conversation.id}
                  type="button"
                  className={`conversation-list__item${conversation.id === selectedId ? ' conversation-list__item--active' : ''}`}
                  onClick={() => setSelectedId(conversation.id)}
                >
                  <strong>{conversationTitle(conversation)}</strong>
                  <span>{conversationSubtitle(conversation)}</span>
                  {conversation.lastMessage && <small>{conversation.lastMessage}</small>}
                </button>
              ))}
            </aside>
            <div className="conversation-panel">
              <h2>{selectedConversation ? conversationTitle(selectedConversation) : t('Rozmowa')}</h2>
              <div className="conversation-messages">
                {messages.length === 0 && <p className="muted">{t('Brak wiadomosci w tej rozmowie.')}</p>}
                {messages.map((message) => (
                  <div
                    key={message.id}
                    className={`conversation-message${isOwnMessage(message) ? ' conversation-message--mine' : ''}`}
                  >
                    <strong className="conversation-message__sender">
                      {isOwnMessage(message) ? t('Ty') : message.senderDisplayName || t('Uzytkownik')}
                    </strong>
                    <p>{message.message}</p>
                    <small>{formatWhen(message.sentAtUtc)}</small>
                  </div>
                ))}
              </div>
              <form className="conversation-compose" onSubmit={submitMessage}>
                <textarea
                  value={draft}
                  onChange={(event) => setDraft(event.target.value)}
                  rows={3}
                  maxLength={4000}
                  placeholder={t('Wpisz wiadomosc...')}
                />
                <button className="btn primary" type="submit" disabled={isSending || !draft.trim()}>
                  {isSending ? t('Wysylanie...') : t('Wyslij')}
                </button>
              </form>
            </div>
          </div>
        )}
      </section>
    </main>
  )
}

export default ConversationsPage
