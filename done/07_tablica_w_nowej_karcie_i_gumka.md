# Tablica w nowej karcie i gumka

## Wykonano

- Dodano osobna strone tablicy `LessonWhiteboardPage` pod trasa `/lessons/:lessonId/whiteboard`.
- W pokoju lekcji usunieto osadzona tablice; zostal czat lekcji oraz przycisk `Otworz tablice`, ktory otwiera tablice w nowej karcie.
- Przeniesiono logike whiteboard realtime do osobnego widoku tablicy.
- Dodano narzedzie `Gumka`.
- Zmieniono narzedzie rysowania na `Pioro`, dzieki czemu z interfejsu zniknal przycisk `Rysuj`.
- Gumka zapisuje biale pociagniecia z wieksza gruboscia, wiec dziala w synchronizacji realtime i przy odtwarzaniu historii tablicy.
- Zachowano czyszczenie tablicy oraz pobieranie tablicy jako PDF w nowym widoku.
- Dodano link powrotu z tablicy do pokoju lekcji.
- Dodano style dla samodzielnego widoku tablicy, paska narzedzi i ukladu pokoju lekcji bez osadzonej tablicy.

## Dlaczego

Tablica potrzebuje wiecej przestrzeni niz panel w pokoju lekcji. Osobna karta daje wygodniejsza prace, a zapis gumki jako zdarzen whiteboard sprawia, ze efekt jest widoczny dla obu uczestnikow i odtwarza sie po odswiezeniu.

## Weryfikacja

- `dotnet build Learnly.sln` zakonczony powodzeniem.
- `npm run build` w katalogu `frontend` zakonczony powodzeniem.
- `npm run lint` w katalogu `frontend` zakonczony powodzeniem.
