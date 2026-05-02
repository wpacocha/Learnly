import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import AppLayout from './components/AppLayout'
import PrivateRoute from './components/PrivateRoute'
import { AuthProvider } from './context/AuthContext'
import DashboardPage from './pages/DashboardPage'
import LessonRoomPage from './pages/LessonRoomPage'
import LessonsPage from './pages/LessonsPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import SearchPage from './pages/SearchPage'
import TutorAvailabilityPage from './pages/TutorAvailabilityPage'
import TutorDetailPage from './pages/TutorDetailPage'
import TutorProfilePage from './pages/TutorProfilePage'

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<AppLayout />}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            <Route element={<PrivateRoute />}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/search" element={<SearchPage />} />
              <Route path="/tutors/:tutorProfileId" element={<TutorDetailPage />} />
              <Route path="/lessons" element={<LessonsPage />} />
              <Route path="/lessons/:lessonId/room" element={<LessonRoomPage />} />
            </Route>

            <Route element={<PrivateRoute allowedRoles={['Tutor']} />}>
              <Route path="/tutor/profile" element={<TutorProfilePage />} />
              <Route path="/tutor/availability" element={<TutorAvailabilityPage />} />
            </Route>

            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
