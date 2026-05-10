# Debug chatu lekcji

## Problem

Wiadomosci chatu oraz zdarzenia whiteboard zapisuja sie w bazie danych, ale po wyjsciu z lekcji i ponownym wejsciu poprzednie wiadomosci nie sa widoczne.

## Oczekiwane zachowanie

Po wejsciu do konkretnej lekcji aplikacja powinna pobrac przez API dane zapisane w bazie danych dla tej lekcji i pokazac je w kolejnosci chronologicznej.

## Zakres

- Sprawdzic endpoint API odpowiedzialny za pobieranie danych lekcji.
- Upewnic sie, ze API zwraca zapisane wiadomosci chatu dla danej lekcji.
- Upewnic sie, ze API zwraca zapisane zdarzenia whiteboard dla danej lekcji, jesli sa potrzebne do odtworzenia tablicy.
- Posortowac dane chronologicznie po czasie utworzenia.
- Podlaczyc pobieranie danych w widoku lekcji po wejściu na strone.
- Zweryfikowac, ze po wyjsciu i ponownym wejsciu do lekcji historia chatu i whiteboard sa odtworzone.
