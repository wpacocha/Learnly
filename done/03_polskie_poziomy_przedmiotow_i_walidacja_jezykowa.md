# Polskie poziomy przedmiotow i walidacja jezykowa

## Wykonano

- Dodano flage `IsLanguage` do encji `Subject`, konfiguracji EF i snapshotu migracji.
- Dodano migracje `20260515100000_AddSubjectLanguageFlagAndPolishLevels`, ktora:
  - dodaje kolumne `IsLanguage` w tabeli `subjects`,
  - oznacza przedmioty jezykowe,
  - dopisuje brakujace przedmioty jezykowe,
  - zmienia angielskie poziomy na polskie odpowiedniki,
  - dopisuje poziomy edukacyjne i poziomy jezykowe `A1-C2`.
- Zmieniono seed katalogu tak, aby byl idempotentny: aktualizuje istniejace przedmioty i poziomy oraz dopisuje brakujace pozycje.
- Rozszerzono `LookupItemDto` dla przedmiotow o informacje, czy przedmiot jest jezykowy.
- Dodano walidacje backendowa w zapisie oferty tutora: poziomy `A1-C2` mozna zapisac tylko dla przedmiotow jezykowych.
- Dodano filtrowanie poziomow w formularzu profilu tutora: dla przedmiotow niejezykowych nie sa pokazywane poziomy `A1-C2`.
- Przy zmianie przedmiotu w pozycji oferty usuwane sa z niej poziomy, ktore po zmianie nie sa juz dozwolone.
- Dodano walidacje frontendowa przed zapisem oferty, aby uzytkownik dostal jasny komunikat jeszcze przed wyslaniem danych do API.

## Dlaczego

Zmiana rozdziela dwa typy poziomow: poziomy edukacyjne dla zwyklych przedmiotow oraz poziomy jezykowe `A1-C2`. Dzieki temu baza nie przyjmuje juz np. matematyki z poziomem `B2`, a interfejs nie proponuje uzytkownikowi niedozwolonych opcji.

## Weryfikacja

- `dotnet build Learnly.sln` zakonczony powodzeniem.
- `npm run build` w katalogu `frontend` zakonczony powodzeniem.
