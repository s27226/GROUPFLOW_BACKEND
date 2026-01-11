# GroupFlow – Backend (ASP.NET Core + GraphQL)

## 1. Opis projektu

GroupFlow to aplikacja webowa wspierająca współpracę użytkowników w ramach projektów. System umożliwia m.in. tworzenie projektów, publikowanie postów, komunikację w czasie rzeczywistym (chat), reakcje, rekomendacje, moderację treści oraz zarządzanie plikami.

Backend aplikacji został zaimplementowany w technologii **ASP.NET Core 8** z wykorzystaniem **Entity Framework Core 9**, **GraphQL (HotChocolate 15)** oraz bazy danych **PostgreSQL**.

Projekt został przygotowany z uwzględnieniem zasad bezpieczeństwa, wydajności oraz dobrych praktyk inżynierii oprogramowania.

---

## 2. Architektura aplikacji

Aplikacja posiada architekturę warstwową:

* **API / GraphQL** – obsługa zapytań i mutacji z jednolitą obsługą błędów
* **Warstwa serwisów** – logika biznesowa, transakcje, walidacja
* **Warstwa dostępu do danych** – Entity Framework Core z async/await
* **Baza danych** – PostgreSQL z connection pooling

Komunikacja z frontendem odbywa się poprzez GraphQL, co umożliwia elastyczne pobieranie danych i ograniczenie nadmiarowych odpowiedzi.

---

## 3. Modele danych i optymalizacja bazy

### 3.1 Indeksy bazodanowe

W projekcie zastosowano indeksy w miejscach, gdzie są one istotne z punktu widzenia wydajności zapytań:

* klucze obce (np. `UserId`, `ProjectId`, `PostId`)
* relacje wiele-do-wielu (np. użytkownicy–projekty, polubienia)
* często filtrowane pola (np. `IsPublic`, `CreatedAt`, `IsRead`)

### 3.2 Spójność danych i transakcje

* zastosowano relacje z kluczami obcymi
* operacje wielokrokowe owinięte w transakcje (`BeginTransactionAsync`)
* wszystkie operacje bazodanowe używają `SaveChangesAsync` z `CancellationToken`
* poprawne rollback przy błędach

---

## 4. Walidacja danych wejściowych

Dane wejściowe przekazywane do mutacji GraphQL są walidowane przy użyciu:

* **DataAnnotations** – `[Required]`, `[StringLength]`, `[EmailAddress]`
* **FluentValidation** – zaawansowane reguły walidacji

Walidacja odbywa się przed wykonaniem logiki biznesowej poprzez `ValidateInput()`.

---

## 5. Obsługa błędów

Aplikacja wykorzystuje jednolity system obsługi błędów:

* **Wyjątki domenowe** (`DomainExceptions.cs`):
  - `AuthenticationException` – brak uwierzytelnienia
  - `AuthorizationException` – brak uprawnień
  - `EntityNotFoundException` – brak encji
  - `DuplicateEntityException` – duplikat
  - `BusinessRuleException` – naruszenie reguły biznesowej
  - `ValidationException` – błędy walidacji

* **GraphQL Error Filter** – automatyczna konwersja wyjątków na spójne błędy GraphQL z kodami HTTP

---

## 6. Logging (Serilog)

Aplikacja wykorzystuje **Serilog** do logowania:

* logowanie do konsoli i plików
* rotating log files (dzienne archiwum)
* logowanie request/response przez `UseSerilogRequestLogging()`
* strukturyzowane logowanie z kontekstem (UserId, ProjectId, etc.)
* różne poziomy dla Microsoft/EF Core (redukcja szumu)

---

## 7. Konfiguracja środowisk

Aplikacja obsługuje różne ustawienia w zależności od środowiska:

* **Development** – szczegółowe błędy, sensitive data logging
* **Production** – zwiększone bezpieczeństwo, brak introspection GraphQL

Zmienne środowiskowe z pliku `.env`:
- `POSTGRES_CONN_STRING` – connection string
- `JWT_SECRET` – klucz JWT
- `CORS_ORIGINS` – dozwolone origins

---

## 8. Connection pooling i async

* `AddDbContextPool` dla wydajności
* `EnableRetryOnFailure` dla odporności
* wszystkie operacje I/O używają `async/await`
* `CancellationToken` we wszystkich metodach async
* brak synchronicznego `SaveChanges()` w kodzie produkcyjnym

---

## 9. Konfiguracja CORS

* **Development** – `http://localhost:3000`
* **Production** – tylko domena frontendu
* Brak `AllowAnyOrigin`

---

## 10. Bezpieczeństwo

* brak wrażliwych danych w repozytorium
* hasła hashowane (BCrypt)
* JWT authentication z tokenami w cookies
* kontrola dostępu w każdej mutacji
* oddzielenie konfiguracji środowisk

---

## 11. Dependency Injection

Aplikacja wykorzystuje DI z constructor injection:

* Scoped services dla DbContext i serwisów biznesowych
* `ILogger<T>` w każdej klasie dla logowania
* `IHttpContextAccessor` dla kontekstu HTTP
* Interfejsy dla głównych serwisów (`IPostService`, `IFriendshipService`)

---

## 12. Uruchomienie projektu

### Wymagania:

* .NET 8+
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

## 13. Autor

Projekt wykonany w ramach pracy dyplomowej.

Autorzy: *[Twoje imię i nazwisko]*
