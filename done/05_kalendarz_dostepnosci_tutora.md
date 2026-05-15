# Kalendarz dostepnosci tutora

## Wykonano

- Przebudowano widok `TutorAvailabilityPage` z pojedynczego pola `datetime-local` na miesieczna kartke kalendarza.
- Dodano nawigacje po miesiacach od obecnego miesiaca do maksymalnie 3 miesiecy do przodu.
- Zablokowano wybieranie dni z przeszlosci oraz dni poza zakresem 3 miesiecy.
- Dodano wybor pozycji oferty, dnia oraz bloku godzin `Od` / `Do`.
- Dodano automatyczny podzial bloku na sloty wedlug `durationMinutes` z wybranej pozycji oferty tutora.
- Dodano szkic slotow: nowe terminy sa widoczne lokalnie, ale nie sa zapisywane w bazie przed kliknieciem `Zapisz`.
- Dodano mozliwosc usuwania pojedynczych slotow ze szkicu oraz czyszczenia calego szkicu.
- Dodano zabezpieczenie przed dodawaniem slotow w przeszlosci oraz przed nakladaniem sie na zapisane lub juz przygotowane sloty.
- Dodano licznik zapisanych i szkicowanych slotow bezposrednio na dniach kalendarza.
- Zostawiono usuwanie zapisanych slotow z listy zapisanej dostepnosci.
- Dodano style dla kalendarza, szkicu i responsywnego ukladu.
- Usunieto nieuzywany stan `reviewOk` w `LessonsPage`, poniewaz blokowal `npm run lint`.

## Dlaczego

Tutor moze teraz planowac dostepnosc naturalnie: wybiera miesiac, dzien i blok godzin, a aplikacja dzieli blok na konkretne lekcje. Szkic chroni przed przypadkowym zapisem, bo zmiany trafiaja do bazy dopiero po swiadomym kliknieciu `Zapisz`.

## Weryfikacja

- `dotnet build Learnly.sln` zakonczony powodzeniem.
- `npm run build` w katalogu `frontend` zakonczony powodzeniem.
- `npm run lint` w katalogu `frontend` zakonczony powodzeniem.
- Proba otwarcia lokalnego widoku w przegladarce aplikacji zostala zablokowana przez polityke przegladarki dla lokalnego adresu, wiec weryfikacja UI zostala ograniczona do builda i lintingu.
