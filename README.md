# GroupFlow – Backend (ASP.NET Core + GraphQL)

## 1. Opis projektu

GroupFlow to aplikacja webowa wspierająca współpracę użytkowników w ramach projektów. System umożliwia m.in. tworzenie projektów, publikowanie postów, komunikację w czasie rzeczywistym (chat), reakcje, rekomendacje, moderację treści oraz zarządzanie plikami.

Backend aplikacji został zaimplementowany w technologii **ASP.NET Core** z wykorzystaniem **Entity Framework Core**, **GraphQL (HotChocolate)** oraz bazy danych **PostgreSQL**.

Projekt został przygotowany z uwzględnieniem zasad bezpieczeństwa, wydajności oraz dobrych praktyk inżynierii oprogramowania.

---

## 2. Architektura aplikacji

Aplikacja posiada architekturę warstwową:

* **API / GraphQL** – obsługa zapytań i mutacji
* **Warstwa logiki biznesowej** – walidacja danych wejściowych, reguły biznesowe
* **Warstwa dostępu do danych** – Entity Framework Core
* **Baza danych** – PostgreSQL

Komunikacja z frontendem odbywa się poprzez GraphQL, co umożliwia elastyczne pobieranie danych i ograniczenie nadmiarowych odpowiedzi.

---

## 3. Modele danych i optymalizacja bazy

### 3.1 Indeksy bazodanowe

W projekcie zastosowano indeksy w miejscach, gdzie są one istotne z punktu widzenia wydajności zapytań, w szczególności:

* klucze obce (np. `UserId`, `ProjectId`, `PostId`)
* relacje wiele-do-wielu (np. użytkownicy–projekty, polubienia)
* często filtrowane pola (np. `IsPublic`, `CreatedAt`, `IsRead`)

Indeksy są definiowane przy użyciu konfiguracji Fluent API w `OnModelCreating`.

### 3.2 Spójność danych

* zastosowano relacje z kluczami obcymi
* wykorzystano kolekcje nawigacyjne
* zadbano o poprawne mapowanie relacji self-referencing (np. komentarze, udostępnione posty)

---

## 4. Walidacja danych wejściowych

Dane wejściowe przekazywane do mutacji GraphQL są walidowane przy użyciu **DataAnnotations**.

Przykłady walidacji:

* `[Required]` – pola wymagane
* `[StringLength]` – ograniczenia długości tekstu
* `[EmailAddress]` – poprawność adresu e-mail

Walidacja odbywa się przed wykonaniem logiki biznesowej, co zapobiega zapisywaniu niepoprawnych danych do bazy.

---

## 5. Konfiguracja środowisk (Development / Production)

Aplikacja obsługuje różne ustawienia konfiguracyjne w zależności od środowiska uruchomieniowego:

* **Development** – lokalne uruchomienie, szczegółowe komunikaty błędów
* **Production** – środowisko produkcyjne, zwiększone bezpieczeństwo

Wykorzystywana jest zmienna środowiskowa:

```
ASPNETCORE_ENVIRONMENT
```

Dzięki temu aplikacja automatycznie wybiera odpowiednie ustawienia bez konieczności modyfikacji kodu.

---

## 6. Connection pooling i dostęp do bazy danych

Połączenie z bazą PostgreSQL realizowane jest za pomocą **Entity Framework Core** oraz provider’a **Npgsql**.

Zastosowano **connection pooling** w celu poprawy wydajności i skalowalności aplikacji:

* `AddDbContextPool`
* jawna konfiguracja minimalnej i maksymalnej liczby połączeń
* obsługa retry policy (`EnableRetryOnFailure`)

Connection stringi przechowywane są w zmiennych środowiskowych (np. plik `.env`), co zwiększa bezpieczeństwo i umożliwia łatwą konfigurację różnych środowisk.

---

## 7. Konfiguracja CORS

W projekcie zastosowano poprawną konfigurację **CORS (Cross-Origin Resource Sharing)**, zależną od środowiska:

* **Development** – dostęp tylko z `http://localhost:3000`
* **Production** – dostęp wyłącznie z domeny frontendu aplikacji

Nie stosowano `AllowAnyOrigin`, co zapobiega nieautoryzowanemu dostępowi do API.

---

## 8. Bezpieczeństwo

* brak wrażliwych danych w repozytorium
* hasła użytkowników przechowywane w postaci haszowanej
* kontrola dostępu po stronie backendu
* oddzielenie konfiguracji środowisk

---

## 9. Uruchomienie projektu

### Wymagania:

* .NET 7+
* PostgreSQL
* Node.js (frontend – opcjonalnie)

### Kroki:

1. Skonfigurować zmienne środowiskowe lub plik `.env`
2. Uruchomić migracje bazy danych
3. Uruchomić aplikację:

```bash
dotnet run
```

---

## 10. Autor

Projekt wykonany w ramach zaliczenia przedmiotu.

Autor: *[Twoje imię i nazwisko]*
