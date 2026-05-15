/* eslint-disable react-refresh/only-export-components -- context module exports hook + provider */
import { createContext, useContext, useEffect, useMemo, useState } from 'react'

const LANGUAGE_KEY = 'learnly_language'
const LanguageContext = createContext(null)

const PL_TO_EN = {
  Panel: 'Dashboard',
  'Znajdz tutora': 'Find a tutor',
  Poziomy: 'Levels',
  'Witaj w Learnly': 'Welcome to Learnly',
  'Zarządzaj lekcjami i profilem z jednego miejsca. Wybierz poniżej, co chcesz zrobić.': 'Manage lessons and your profile from one place. Choose what you want to do below.',
  Konto: 'Account',
  Role: 'Roles',
  'Szybkie przejścia': 'Quick links',
  'Najczęstsze akcje w jednym rzucie oka.': 'Common actions at a glance.',
  'Znajdź tutora': 'Find a tutor',
  'Przeglądaj korepetytorów i filtruj wyniki': 'Browse tutors and filter results',
  'Lista rezerwacji i pokój lekcyjny': 'Booking list and lesson room',
  'Profil i oferta': 'Profile and offer',
  'Opis, przedmioty, poziomy nauczania': 'Bio, subjects and teaching levels',
  'Kalendarz dostępności': 'Availability calendar',
  'Dodaj sloty, które uczeń może zarezerwować': 'Add slots students can book',
  Szukaj: 'Search',
  Chat: 'Chat',
  Powiadomienia: 'Notifications',
  Lekcje: 'Lessons',
  'Profil tutora': 'Tutor profile',
  Wyloguj: 'Log out',
  Logowanie: 'Login',
  Rejestracja: 'Register',
  'Zalogowano jako': 'Signed in as',
  'Moje lekcje': 'My lessons',
  'Pokój lekcyjny': 'Lesson room',
  'Czat lekcji': 'Lesson chat',
  Tablica: 'Whiteboard',
  'Otwórz tablicę': 'Open whiteboard',
  'Tablica otwiera się w osobnej karcie, żeby było więcej miejsca na pracę.': 'The whiteboard opens in a separate tab to give you more workspace.',
  'Chat z tutorami': 'Chat with tutors',
  'Pisz z prowadzacym niezaleznie od ustalonej lekcji.': 'Message a tutor outside a scheduled lesson.',
  'Brak rozmow. Wejdz w profil tutora i kliknij „Napisz do tutora”.': 'No conversations yet. Open a tutor profile and click "Message tutor".',
  'Wpisz wiadomosc...': 'Type a message...',
  Wysyłanie: 'Sending',
  Wysylanie: 'Sending',
  'Brak powiadomien.': 'No notifications.',
  'Oznacz jako przeczytane': 'Mark as read',
  Przejdz: 'Open',
  'Twoja dostępność': 'Your availability',
  'Twoja dostÄ™pnoĹ›Ä‡': 'Your availability',
  'Pozycja oferty': 'Offer item',
  Od: 'From',
  Do: 'To',
  'Dodaj blok do szkicu': 'Add block to draft',
  Zapisz: 'Save',
  'Wyczyść szkic': 'Clear draft',
  Poprzedni: 'Previous',
  Następny: 'Next',
  Pon: 'Mon',
  Wt: 'Tue',
  Śr: 'Wed',
  Czw: 'Thu',
  Pt: 'Fri',
  Sob: 'Sat',
  Nd: 'Sun',
  'Zapisane sloty': 'Saved slots',
  'Brak zdefiniowanej dostępności.': 'No availability defined.',
  'Profil został zaktualizowany.': 'Profile has been updated.',
  'Profil został utworzony. Dodaj poniżej przedmioty, tryb i stawki dla każdej pozycji oferty.': 'Profile has been created. Add subjects, mode and rates for each offer item below.',
  'Przedmioty, tryb i poziomy': 'Subjects, mode and levels',
  'Dodaj pozycję oferty': 'Add offer item',
  'Zapisz ofertę (przedmioty i poziomy)': 'Save offer (subjects and levels)',
  'Poziomy nauczania': 'Teaching levels',
  Przedmiot: 'Subject',
  Tryb: 'Mode',
  'Stawka godzinowa (PLN)': 'Hourly rate (PLN)',
  'Długość lekcji': 'Lesson duration',
  Online: 'Online',
  Stacjonarnie: 'In person',
  'Online i stacjonarnie': 'Online and in person',
  'Szukaj tutorów': 'Search tutors',
  'Dostępne terminy': 'Available slots',
  Rezerwuj: 'Book',
  Rezerwacja: 'Booking',
  'Opinie uczniów': 'Student reviews',
  'Brak opinii': 'No reviews',
  Pióro: 'Pen',
  Gumka: 'Eraser',
  Wyczyść: 'Clear',
  PDF: 'PDF',
  'Rozmowa głosowa': 'Voice call',
  '← Pokój lekcyjny': '← Lesson room',
  '← Moje lekcje': '← My lessons',
  'Szkoła podstawowa (1-3)': 'Primary school (1-3)',
  'Szkola podstawowa (1-3)': 'Primary school (1-3)',
  'Szkoła podstawowa (4-8)': 'Primary school (4-8)',
  'Szkola podstawowa (4-8)': 'Primary school (4-8)',
  'Egzamin ósmoklasisty': 'Eighth-grade exam',
  'Egzamin osmoklasisty': 'Eighth-grade exam',
  'Szkoła średnia': 'Secondary school',
  'Szkola srednia': 'Secondary school',
  'Poziom maturalny': 'Matura level',
  'Poziom maturalny rozszerzony': 'Advanced matura level',
  Studia: 'University',
  'Kurs dla dorosłych': 'Adult course',
  'Kurs dla doroslych': 'Adult course',
  Matematyka: 'Mathematics',
  'Język angielski': 'English',
  'Jezyk angielski': 'English',
  'Język polski': 'Polish',
  'Jezyk polski': 'Polish',
  'Język niemiecki': 'German',
  'Jezyk niemiecki': 'German',
  'Język hiszpański': 'Spanish',
  'Jezyk hiszpanski': 'Spanish',
  Fizyka: 'Physics',
  Chemia: 'Chemistry',
  Biologia: 'Biology',
  Informatyka: 'Computer science',
  Historia: 'History',
  Geografia: 'Geography',
  Uczeń: 'Student',
  Uczen: 'Student',
  Tutor: 'Tutor',
  Administrator: 'Administrator',
  'Glowna nawigacja': 'Main navigation',
  Haslo: 'Password',
  Rola: 'Role',
  'Ladowanie sesji uzytkownika...': 'Loading user session...',
  'Ladowanie rozmow...': 'Loading conversations...',
  'Ladowanie powiadomien...': 'Loading notifications...',
  'Ladowanie lekcji...': 'Loading lessons...',
  'Ladowanie profilu...': 'Loading profile...',
  'Ladowanie profilu tutora...': 'Loading tutor profile...',
  'Ladowanie katalogu...': 'Loading catalog...',
  'Nie udalo sie pobrac powiadomien.': 'Could not load notifications.',
  'Nie udalo sie oznaczyc powiadomienia jako przeczytane.': 'Could not mark notification as read.',
  'Nie udalo sie pobrac katalogu przedmiotow i poziomow.': 'Could not load subjects and levels.',
  'Wyszukiwanie nie powiodlo sie. Sprawdz, czy API dziala.': 'Search failed. Check whether the API is running.',
  'Uzyj filtrow ponizej, a pod nimi zobaczysz dopasowanych korepetytorow.': 'Use the filters below to see matching tutors.',
  Wszystkie: 'All',
  Poziom: 'Level',
  Dowolny: 'Any',
  Dowolna: 'Any',
  Lokalizacja: 'Location',
  'Opcjonalnie - glownie dla stacjonarnie / hybryda': 'Optional - mostly for in-person / hybrid',
  'np. Warszawa': 'e.g. Warsaw',
  'Dostepny od': 'Available from',
  'Ocena tutora': 'Tutor rating',
  'Zakres stawki godzinowej': 'Hourly rate range',
  'Minimalna stawka': 'Minimum rate',
  'Maksymalna stawka': 'Maximum rate',
  'Stawka (PLN/h)': 'Rate (PLN/h)',
  'Szukam...': 'Searching...',
  'Wyczysc filtry': 'Clear filters',
  'Brak wynikow przy tych kryteriach.': 'No results for these criteria.',
  'Wyniki wyszukiwania': 'Search results',
  'Brak wynikow': 'No results',
  'Poszerz zakres stawki, usun filtr daty albo wybierz inny przedmiot. Upewnij sie tez, ze tutorzy maja wypelniona dostepnosc.': 'Broaden the rate range, remove the date filter, or choose another subject. Also make sure tutors have availability filled in.',
  'Poszerz zakres stawki, usun filtr daty albo wybierz inny przedmiot. Upewnij sie tez, ze tutorzy maja': 'Broaden the rate range, remove the date filter, or choose another subject. Also make sure tutors have',
  'wypelniona dostepnosc.': 'availability filled in.',
  'Profil i rezerwacja': 'Profile and booking',
  Przedmioty: 'Subjects',
  'Wolne sloty': 'Available slots',
  Oferta: 'Offer',
  'Napisz do tutora': 'Message tutor',
  'Dostepne terminy': 'Available slots',
  'Zaloguj sie jako uczen (Student), aby zarezerwowac termin.': 'Sign in as a student to book a slot.',
  'Brak przyszlych slotow w tym wyniku wyszukiwania.': 'No future slots in this search result.',
  'Poprzedni tydzien': 'Previous week',
  'Nastepny tydzien': 'Next week',
  'Brak slotow': 'No slots',
  'Opinie uczniow': 'Student reviews',
  'Srednia ocen': 'Average rating',
  'Brak ocen': 'No ratings',
  opinia: 'review',
  opinii: 'reviews',
  'Tylko uczen moze rezerwowac lekcje.': 'Only a student can book lessons.',
  'Rezerwacja zapisana. Zobacz "Moje lekcje".': 'Booking saved. See "My lessons".',
  'Nie udalo sie zarezerwowac terminu.': 'Could not book the slot.',
  'Wroc do wyszukiwania': 'Back to search',
  Wstecz: 'Back',
  Wiadomosc: 'Message',
  'Wiadomosc...': 'Message...',
  Wyslij: 'Send',
  Rozmowy: 'Conversations',
  Rozmowa: 'Conversation',
  Ty: 'You',
  Uzytkownik: 'User',
  'Brak wiadomosci w tej rozmowie.': 'No messages in this conversation.',
  'Nie udalo sie pobrac rozmow.': 'Could not load conversations.',
  'Nie udalo sie pobrac wiadomosci.': 'Could not load messages.',
  'Nie udalo sie wyslac wiadomosci.': 'Could not send the message.',
  'Twoja dostepnosc': 'Your availability',
  'brak oferty': 'no offer',
  'Szkic dla dnia:': 'Draft for day:',
  Usun: 'Remove',
  'Terminy dla tej pozycji': 'Slots for this offer item',
  'Ustal dostepne godziny': 'Set available hours',
  'Poczatek slotu': 'Slot start',
  'Imie': 'First name',
  Nazwisko: 'Last name',
  'Opis (bio)': 'Bio',
  'Doswiadczenie, styl pracy, czego uczysz.': 'Experience, teaching style, what you teach.',
  'Zdjecie profilowe - opcjonalne': 'Profile photo - optional',
  'Usun zdjecie': 'Remove photo',
  'Zapisz profil': 'Save profile',
  Anuluj: 'Cancel',
  Edytuj: 'Edit',
  'Zapisana oferta': 'Saved offer',
  'wybierz': 'select',
  'Lokalizacja (stacjonarnie)': 'Location (in person)',
  'np. Warszawa, Srodmiescie': 'e.g. Warsaw, City Center',
  'Ocena (1-5)': 'Rating (1-5)',
  'Komentarz (opcjonalnie)': 'Comment (optional)',
  'Ustaw ocene do': 'Set rating to',
  'Ocena': 'Rating',
  'Otworz pokoj': 'Open room',
  'Odwołaj': 'Cancel',
  'Odwolaj': 'Cancel',
  'Dodaj opinie': 'Add review',
  'Pokoj lekcyjny': 'Lesson room',
  'Narzędzia tablicy': 'Whiteboard tools',
  'Narzedzia tablicy': 'Whiteboard tools',
  Narzedzie: 'Tool',
  Kolor: 'Color',
  'Zaloguj sie, aby korzystac z lekcji i profilu w Learnly.': 'Sign in to use lessons and your Learnly profile.',
  'Nie udalo sie zalogowac. Sprawdz dane i sprobuj ponownie.': 'Could not sign in. Check your details and try again.',
  'Zaloguj sie': 'Sign in',
  'Logowanie...': 'Signing in...',
  'Nie masz konta?': 'Do not have an account?',
  'Zaloz konto': 'Create account',
  'Utworz konto ucznia lub tutora i zacznij korzystac z platformy.': 'Create a student or tutor account and start using the platform.',
  'Nie udalo sie utworzyc konta. Sprawdz dane i sprobuj ponownie.': 'Could not create the account. Check your details and try again.',
  'Tworzenie konta...': 'Creating account...',
  'Masz juz konto?': 'Already have an account?',
  'Jako uczen lub tutor widzisz lekcje powiazane z kontem. Status i szczegoly sa zawsze aktualne.': 'As a student or tutor, you can see lessons linked to your account. Status and details are always up to date.',
  'Nie masz jeszcze zadnych lekcji.': 'You do not have any lessons yet.',
  Oczekuje: 'Pending',
  Potwierdzona: 'Confirmed',
  Zakonczona: 'Completed',
  Anulowana: 'Cancelled',
  Start: 'Start',
  Koniec: 'End',
  Status: 'Status',
  Identyfikator: 'Identifier',
  'Oznacz jako zakonczona': 'Mark as completed',
  'Wystawiono opinie': 'Review submitted',
  'Ocen lekcje': 'Review lesson',
  'Ocen prowadzacego': 'Review tutor',
  Lekcja: 'Lesson',
  'Wyslij opinie': 'Send review',
  'Nie udalo sie pobrac lekcji.': 'Could not load lessons.',
  'Nie udalo sie odswiezyc lekcji.': 'Could not refresh lessons.',
  'Zmiana statusu nie powiodla sie.': 'Could not change status.',
  'Nie udalo sie zapisac opinii.': 'Could not save review.',
  'Bazy Danych': 'Databases',
  'Bazy danych': 'Databases',
  'Rachunek rozniczkowy': 'Differential calculus',
  'Rachunek różniczkowy': 'Differential calculus',
  'Jezyk japonski': 'Japanese',
  'Język japoński': 'Japanese',
  'j. japonski': 'Japanese',
  'j. japoński': 'Japanese',
  Geometria: 'Geometry',
  WOS: 'Civics',
  Przyroda: 'Nature',
  'Jezyk wloski': 'Italian',
  'Język włoski': 'Italian',
  'j. wloski': 'Italian',
  'j. włoski': 'Italian',
  poziomy: 'levels',
  'poziomy:': 'levels:',
  'Nowa wiadomosc od': 'New message from',
  'Nowe wiadomosci od': 'New messages from',
  Ostatnia: 'Latest',
  'Zabookowano lekcje na': 'Lesson booked for',
  'Lekcja z terminu': 'Lesson scheduled for',
  'zostala odwolana.': 'was cancelled.',
  'Przypomnienie: lekcja zaczyna sie o': 'Reminder: lesson starts at',
}

const normalizeTextKey = (text) =>
  String(text ?? '')
    .trim()
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .replace(/[–—]/g, '-')
    .replace(/[„”]/g, '"')
    .replace(/\s+/g, ' ')
    .toLowerCase()

const NORMALIZED_PL_TO_EN = Object.entries(PL_TO_EN).reduce((acc, [pl, en]) => {
  acc[normalizeTextKey(pl)] = en
  return acc
}, {})

const MONTHS_PL_TO_EN = {
  stycznia: 'January',
  styczeń: 'January',
  luty: 'February',
  lutego: 'February',
  marzec: 'March',
  marca: 'March',
  kwiecień: 'April',
  kwietnia: 'April',
  maj: 'May',
  maja: 'May',
  czerwiec: 'June',
  czerwca: 'June',
  lipiec: 'July',
  lipca: 'July',
  sierpień: 'August',
  sierpnia: 'August',
  wrzesień: 'September',
  września: 'September',
  październik: 'October',
  października: 'October',
  listopad: 'November',
  listopada: 'November',
  grudzień: 'December',
  grudnia: 'December',
}

const EN_TO_PL = Object.entries(PL_TO_EN).reduce((acc, [pl, en]) => {
  acc[en] = pl
  return acc
}, {})

const MONTHS_EN_TO_PL = Object.entries(MONTHS_PL_TO_EN).reduce((acc, [pl, en]) => {
  acc[en] = pl
  return acc
}, {})

const replaceMonths = (text, language) => {
  let output = text
  if (language === 'en') {
    Object.entries(MONTHS_PL_TO_EN).forEach(([pl, en]) => {
      output = output.replace(new RegExp(`\\b${pl}\\b`, 'gi'), en)
    })
  } else {
    Object.entries(MONTHS_EN_TO_PL).forEach(([en, pl]) => {
      output = output.replace(new RegExp(`\\b${en}\\b`, 'g'), pl)
    })
  }
  return output
}

const translateText = (text, language) => {
  if (!text) {
    return text
  }
  const trimmed = text.trim()
  const translated = language === 'en' ? PL_TO_EN[trimmed] ?? NORMALIZED_PL_TO_EN[normalizeTextKey(trimmed)] : EN_TO_PL[trimmed]
  if (translated) {
    return text.replace(trimmed, translated)
  }
  if (language === 'en') {
    let output = text
    output = output.replace(/Nowa wiadomosc od (.*?):/i, 'New message from $1:')
    output = output.replace(/Nowe wiadomosci od (.*?)\. Ostatnia:/i, 'New messages from $1. Latest:')
    output = output.replace(/Zabookowano lekcje na /i, 'Lesson booked for ')
    output = output.replace(/Lekcja z terminu (.*?) zostala odwolana\./i, 'Lesson scheduled for $1 was cancelled.')
    output = output.replace(/Przypomnienie: lekcja zaczyna sie o /i, 'Reminder: lesson starts at ')
    if (output !== text) {
      return replaceMonths(output, language)
    }
  }
  return replaceMonths(text, language)
}

const translateNode = (node, language) => {
  if (node.nodeType === Node.TEXT_NODE) {
    const translated = translateText(node.nodeValue, language)
    if (translated !== node.nodeValue) {
      node.nodeValue = translated
    }
    return
  }

  if (node.nodeType !== Node.ELEMENT_NODE) {
    return
  }

  const element = node
  ;['placeholder', 'aria-label', 'title', 'value'].forEach((attr) => {
    if (element.hasAttribute?.(attr)) {
      const current = element.getAttribute(attr)
      const translated = translateText(current, language)
      if (translated !== current) {
        element.setAttribute(attr, translated)
      }
    }
  })

  node.childNodes?.forEach((child) => translateNode(child, language))
}

export const LanguageProvider = ({ children }) => {
  const [language, setLanguage] = useState(() => localStorage.getItem(LANGUAGE_KEY) || 'pl')

  useEffect(() => {
    localStorage.setItem(LANGUAGE_KEY, language)
    document.documentElement.lang = language
  }, [language])

  const value = useMemo(
    () => ({
      language,
      setLanguage,
      toggleLanguage: () => setLanguage((current) => (current === 'pl' ? 'en' : 'pl')),
      t: (text) => translateText(text, language),
    }),
    [language],
  )

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>
}

export const LanguageTranslator = () => {
  const { language } = useLanguage()

  useEffect(() => {
    const root = document.getElementById('root')
    if (!root) {
      return undefined
    }

    translateNode(root, language)
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        mutation.addedNodes.forEach((node) => translateNode(node, language))
        if (mutation.type === 'characterData') {
          translateNode(mutation.target, language)
        }
      })
    })
    observer.observe(root, { childList: true, subtree: true, characterData: true })
    return () => observer.disconnect()
  }, [language])

  return null
}

export const useLanguage = () => {
  const context = useContext(LanguageContext)
  if (!context) {
    throw new Error('useLanguage must be used within LanguageProvider')
  }
  return context
}
