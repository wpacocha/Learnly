# Zmiana jezyka systemowego PL/ENG

## Wykonano

- Dodano `LanguageProvider` i globalny stan jezyka aplikacji.
- Dodano zapis wyboru jezyka w `localStorage` pod kluczem `learnly_language`.
- Dodano `LanguageTranslator`, ktory tlumaczy widoczne teksty, placeholdery, `aria-label`, `title` i wartosci atrybutow w aktualnie wyrenderowanej aplikacji.
- Dodano slownik PL/ENG obejmujacy:
  - nawigacje,
  - przyciski,
  - karty i naglowki,
  - dni tygodnia,
  - miesiace,
  - poziomy,
  - przedmioty,
  - typy uzytkownikow,
  - czat,
  - powiadomienia,
  - lekcje,
  - profil tutora,
  - kalendarz dostepnosci,
  - tablice.
- Dodano przelacznik jezyka w gornym pasku aplikacji jako flage `PL` / `ENG`.
- Przelacznik dziala dla zalogowanych i niezalogowanych uzytkownikow.
- Ustawiany jest atrybut `lang` na elemencie `html`.
- Tlumaczenie dziala w obie strony, wiec po powrocie na PL teksty wracaja z angielskiego na polski.
- Dodano style dla przelacznika jezyka.

## Dlaczego

Aplikacja potrzebowala jednego globalnego mechanizmu zmiany jezyka zamiast lokalnych, rozproszonych tlumaczen. Warstwa na poziomie aplikacji pozwala objac istniejace widoki bez przepisywania calego frontendu naraz i daje punkt startowy do dalszego rozwijania slownikow.

## Weryfikacja

- `dotnet build Learnly.sln` zakonczony powodzeniem.
- `npm run build` w katalogu `frontend` zakonczony powodzeniem.
- `npm run lint` w katalogu `frontend` zakonczony powodzeniem.
