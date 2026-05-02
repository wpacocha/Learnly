import * as signalR from '@microsoft/signalr'
import { API_BASE_URL, TOKEN_KEY } from '../api/client'

const tokenFactory = () => localStorage.getItem(TOKEN_KEY) ?? ''

export const createLessonChatConnection = () =>
  new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/lesson-chat`, {
      accessTokenFactory: tokenFactory,
    })
    .withAutomaticReconnect()
    .build()

export const createWhiteboardConnection = () =>
  new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/whiteboard`, {
      accessTokenFactory: tokenFactory,
    })
    .withAutomaticReconnect()
    .build()
