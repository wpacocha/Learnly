# Czat poza lekcja i powiadomienia

## Wykonano

- Dodano modele domenowe dla rozmow poza lekcja:
  - `TutorConversation`,
  - `TutorConversationMessage`.
- Dodano model `UserNotification` dla powiadomien uzytkownika.
- Dodano konfiguracje EF i migracje `20260515103000_AddConversationsAndNotifications` tworzaca tabele:
  - `tutor_conversations`,
  - `tutor_conversation_messages`,
  - `user_notifications`.
- Dodano serwis rozmow z obsluga:
  - listy rozmow uzytkownika,
  - utworzenia rozmowy z tutorem,
  - pobrania historii wiadomosci,
  - wyslania wiadomosci.
- Dodano endpointy `/api/conversations`.
- Dodano serwis powiadomien z obsluga:
  - listy powiadomien,
  - oznaczania jako przeczytane,
  - tworzenia przypomnien o lekcji do 30 minut przed startem, jesli jeszcze nie istnieja.
- Dodano endpointy `/api/notifications`.
- Podlaczono powiadomienie dla tutora po zabookowaniu lekcji.
- Podlaczono powiadomienie dla ucznia po odwolaniu lekcji.
- Dodano frontendowa strone `Chat z tutorami`:
  - lista rozmow,
  - historia wiadomosci,
  - wysylanie wiadomosci poza lekcja.
- Dodano frontendowa strone `Powiadomienia`:
  - lista powiadomien,
  - link do szczegolow,
  - oznaczanie jako przeczytane.
- Dodano nawigacje do `Chat` i `Powiadomienia`.
- Dodano przycisk `Napisz do tutora` na profilu tutora dla ucznia, ktory tworzy albo otwiera rozmowe z tym tutorem.
- Dodano style dla listy rozmow, panelu chatu i powiadomien.

## Dlaczego

Uczen moze teraz napisac do prowadzacego bez ustalonej lekcji, a tutor widzi te rozmowy i moze odpisac. Powiadomienia sa trwale zapisane w bazie, wiec rezerwacja, odwolanie i przypomnienie o lekcji nie znikaja po odswiezeniu strony.

## Weryfikacja

- `dotnet build Learnly.sln` zakonczony powodzeniem.
- `npm run build` w katalogu `frontend` zakonczony powodzeniem.
- `npm run lint` w katalogu `frontend` zakonczony powodzeniem.
