# Portal do zarządzania fakturami w KSeF

Portal internetowy do zarządzania fakturami sprzedaży, kontrahentami oraz integracją z Krajowym Systemem e-Faktur (KSeF). Projekt został zrealizowany w technologii **Blazor Web App (.NET 8/9)** z wykorzystaniem komponentów **Radzen Blazor**.

---

## Informacja o projekcie i repozytorium 

Kod źródłowy w tym repozytorium stanowi wycinek większego systemu komercyjnego rozwijanego podczas moich **praktyk programistycznych**. 

W celach ochronnych oraz z poszanowania prywatności firmy i pozostałych członków zespołu:
* **Usunąłem dane wrażliwe**: dane dostępowe do bazy, produkcyjne NIP-y, tokeny sesji i certyfikaty autoryzacyjne KSeF.
* **Ograniczyłem zakres kodu**: repozytorium skupia się na części, za którą bezpośrednio odpowiadałem (warstwa interfejsu użytkownika, komponenty webowe, integracja widoków). Usunąłem również zewnętrzne biblioteki modeli backendowych oraz parser struktur XML KSeF tworzony przez innych programistów.
---

## Mój zakres odpowiedzialności i wdrożone funkcjonalności

W ramach prac nad aplikacją webową zrealizowałem m.in.:

* **Interfejs użytkownika (UI/UX)**:
  * Budowa i dopracowanie responsywnego układu strony (`MainLayout`) opartego o Radzen Layout.
  * Implementacja dwustronnego paska nawigacji oraz wskaźników stanu połączenia (status bramek KSeF oraz zewnętrznego API).
  * Standaryzacja motywów (wsparcie trybu jasnego/ciemnego za pomocą JS interop).

* **Zarządzanie dokumentami sprzedaży**:
  * Widoki tabelaryczne faktur z sortowaniem, filtrowaniem i stronicowaniem.
  * Okna dialogowe szczegółów faktury oraz podglądu pozycji towarowych.
  * Logika ponawiania wysyłki dokumentów oczekujących do bramki KSeF z poziomu panelu akcji.

* **Moduły pomocnicze**:
  * Karty zarządzania kontrahentami i kartoteką towarową.
  * System powiadomień i stanów ładowania (`LoadingService`) blokujący interfejs podczas zapytań asynchronicznych.

---

## Technologie

* **Framework**: .NET 8 / Blazor Web App (Interactive Server)
* **UI Components**: Radzen.Blazor
* **Języki**: C#, Razor, HTML5, CSS3, JavaScript
* **Baza danych**: Entity Framework Core (konfiguracja demonstracyjna)

---
