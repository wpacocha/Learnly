# Obsluga bledow zapisu i przeplyw profilu tutora

## Wykonano

- Dodano wspolna funkcje `getApiErrorMessage`, ktora wyciaga czytelny komunikat z odpowiedzi API: `message`, `detail`, `title`, `errors` albo tresc tekstowa.
- Podlaczono te funkcje do zapisu profilu tutora, przesylania zdjecia, zapisu oferty oraz operacji dostepnosci.
- Dodano przewijanie do gory strony po udanym zapisie profilu i po udanym zapisie oferty.
- Dodano przewijanie do gory takze przy bledach walidacji profilu i oferty, aby uzytkownik od razu widzial komunikat.
- Zablokowano dodawanie i edycje przedmiotow/oferty, dopoki profil tutora nie zostanie utworzony.
- Dodano komunikat w sekcji oferty wyjasniajacy, ze najpierw trzeba zapisac podstawowy profil tutora.
- Zabezpieczono przycisk dodawania pozycji oferty tak, aby nie dodawal przedmiotow przed utworzeniem profilu.

## Dlaczego

Tutor musi miec zapisany profil, zanim aplikacja moze bezpiecznie powiazac z nim przedmioty, poziomy, stawki i terminy. Jednolity parser bledow sprawia, ze uzytkownik widzi przyczyne niepowodzenia zamiast ogolnego komunikatu, a przewiniecie do gory pokazuje status zapisu od razu po akcji.

## Weryfikacja

- `dotnet build Learnly.sln` zakonczony powodzeniem.
- `npm run build` w katalogu `frontend` zakonczony powodzeniem.
