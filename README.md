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

## 3. Wymagania systemowe

### Wymagania wstępne:
- **.NET 8.0 SDK** lub nowszy
- **PostgreSQL 12+** z obsługą JSON
- **Node.js 16+** (dla frontendu – opcjonalnie)
- **Git** (do klonowania repozytorium)

### Zalecane narzędzia:
- **Visual Studio 2022** lub **VS Code** z rozszerzeniami C#
- **pgAdmin** lub **DBeaver** do zarządzania bazą danych
- **Postman** lub **Insomnia** do testowania API

---

## 4. Instalacja i konfiguracja

### Krok 1: Klonowanie repozytorium

```bash
git clone <repository-url>
cd GROUPFLOW_BACKEND
```

### Krok 2: Konfiguracja zmiennych środowiskowych

Utwórz plik `.env` w katalogu głównym projektu:

```env
# Database Configuration
POSTGRES_CONN_STRING_DEV=Host=localhost;Port=5432;Database=groupflow_dev;Username=your_username;Password=your_password
POSTGRES_CONN_STRING_PROD=Host=your-prod-host;Port=5432;Database=groupflow_prod;Username=prod_user;Password=prod_password

# JWT Configuration
JWT_SECRET=your-super-secure-jwt-secret-key-minimum-256-bits

# AWS S3 Configuration (opcjonalnie)
AWS_ACCESS_KEY_ID=your-aws-access-key
AWS_SECRET_ACCESS_KEY=your-aws-secret-key
AWS_REGION=us-east-1
AWS_S3_BUCKET_NAME=your-bucket-name

# Email Configuration (opcjonalnie)
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
```

### Krok 3: Konfiguracja bazy danych PostgreSQL

1. Zainstaluj PostgreSQL na swoim systemie
2. Utwórz bazę danych:
```sql
CREATE DATABASE groupflow_dev;
CREATE USER groupflow_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE groupflow_dev TO groupflow_user;
```

### Krok 4: Instalacja zależności i migracje

```bash
# Przywróć pakiety NuGet
dotnet restore

# Uruchom migracje bazy danych (tylko w środowisku deweloperskim)
dotnet ef database update

# Zbuduj projekt
dotnet build
```

### Krok 5: Uruchomienie aplikacji

```bash
# W środowisku deweloperskim (z automatycznym seedowaniem danych testowych)
dotnet run

# Lub uruchom z określonym środowiskiem
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

Aplikacja będzie dostępna pod adresem: `http://localhost:5000/graphql`

---

## 5. GraphQL API Dokumentacja

### Endpoint główny
- **URL**: `http://localhost:5000/graphql`
- **Playground**: `http://localhost:5000/graphql` (dostępny tylko w Development)

### Autoryzacja
Większość operacji wymaga autoryzacji poprzez JWT token. Token należy przesłać w nagłówku:
```
Authorization: Bearer <your-jwt-token>
```

### Przykład logowania i uzyskania tokena

```graphql
mutation Login {
  login(input: {
    email: "alice@example.com"
    password: "123"
  }) {
    token
    user {
      id
      name
      email
    }
  }
}
```

---

## 6. GraphQL Schema - Przykłady zapytań

### 6.1 Zapytania (Queries)

#### Pobieranie wszystkich użytkowników
```graphql
query GetAllUsers {
  allusers {
    id
    name
    surname
    nickname
    email
    profilePic
    bannerPic
    joined
    skills {
      name
    }
    interests {
      name
    }
  }
}
```

#### Pobieranie pojedynczego użytkownika
```graphql
query GetUser($userId: Int!) {
  getuserbyid(id: $userId) {
    id
    name
    surname
    nickname
    email
    profilePic
    bannerPic
    skills {
      name
    }
    interests {
      name
    }
  }
}
```

#### Pobieranie własnych danych (wymaga autoryzacji)
```graphql
query GetCurrentUser {
  me {
    id
    name
    surname
    nickname
    email
    profilePic
    bannerPic
    skills {
      name
    }
    interests {
      name
    }
  }
}
```

#### Wyszukiwanie użytkowników
```graphql
query SearchUsers($query: String!) {
  searchusers(query: $query) {
    id
    name
    surname
    nickname
    email
    profilePic
  }
}
```

#### Rekomendacje użytkowników
```graphql
query GetSuggestedUsers {
  suggestedusers {
    user {
      id
      name
      surname
      nickname
      profilePic
    }
    matchScore
    commonSkills
    commonInterests
  }
}
```

#### Pobieranie postów z paginacją
```graphql
query GetPosts($first: Int, $after: String) {
  posts(first: $first, after: $after) {
    edges {
      node {
        id
        content
        createdAt
        user {
          id
          name
          nickname
        }
        likes {
          id
          userId
        }
        comments {
          id
          content
          user {
            id
            name
          }
        }
      }
      cursor
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
  }
}
```

#### Pobieranie projektów
```graphql
query GetProjects {
  projects {
    id
    name
    description
    isPublic
    createdAt
    owner {
      id
      name
      nickname
    }
    collaborators {
      id
      name
      nickname
    }
  }
}
```

### 6.2 Mutacje (Mutations)

#### Rejestracja nowego użytkownika
```graphql
mutation RegisterUser {
  register(input: {
    name: "Jan"
    surname: "Kowalski"
    nickname: "janek"
    email: "jan@example.com"
    password: "securePassword123"
  }) {
    id
    name
    email
  }
}
```

#### Logowanie
```graphql
mutation LoginUser {
  login(input: {
    email: "jan@example.com"
    password: "securePassword123"
  }) {
    token
    user {
      id
      name
      email
    }
  }
}
```

#### Tworzenie projektu
```graphql
mutation CreateProject {
  createproject(input: {
    name: "Nowy Projekt"
    description: "Opis projektu"
    isPublic: true
    skills: ["C#", "React"]
    interests: ["Web Development", "AI"]
  }) {
    id
    name
    description
    createdAt
  }
}
```

#### Dodanie posta
```graphql
mutation CreatePost {
  createpost(input: {
    content: "Treść mojego posta"
    projectId: 1
  }) {
    id
    content
    createdAt
    user {
      id
      name
    }
  }
}
```

#### Dodanie komentarza do posta
```graphql
mutation AddComment {
  createpostcomment(input: {
    postId: 1
    content: "Świetny post!"
  }) {
    id
    content
    createdAt
    user {
      id
      name
    }
  }
}
```

#### Polubienie posta
```graphql
mutation LikePost {
  likepost(input: {
    postId: 1
  }) {
    id
    userId
    postId
  }
}
```

#### Zapisywanie posta
```graphql
mutation SavePost {
  savepost(input: {
    postId: 1
  }) {
    id
    userId
    postId
  }
}
```

#### Wysyłanie wiadomości w czacie
```graphql
mutation SendMessage {
  createentry(input: {
    chatId: 1
    content: "Wiadomość testowa"
  }) {
    id
    content
    createdAt
    user {
      id
      name
    }
  }
}
```

---

## 7. Modele danych i optymalizacja bazy

### 7.1 Indeksy bazodanowe

W projekcie zastosowano indeksy w miejscach, gdzie są one istotne z punktu widzenia wydajności zapytań, w szczególności:

* klucze obce (np. `UserId`, `ProjectId`, `PostId`)
* relacje wiele-do-wielu (np. użytkownicy–projekty, polubienia)
* często filtrowane pola (np. `IsPublic`, `CreatedAt`, `IsRead`)

Indeksy są definiowane przy użyciu konfiguracji Fluent API w `OnModelCreating`.

### 7.2 Spójność danych

* zastosowano relacje z kluczami obcymi
* wykorzystano kolekcje nawigacyjne
* zadbano o poprawne mapowanie relacji self-referencing (np. komentarze, udostępnione posty)

---

## 8. Walidacja danych wejściowych

Dane wejściowe przekazywane do mutacji GraphQL są walidowane przy użyciu **DataAnnotations**.

Przykłady walidacji:
```csharp
[property: Required]
[property: StringLength(50)]
string Name,

[property: Required]
[property: EmailAddress]
string Email,

[property: Required]
[property: MinLength(8)]
string Password
```

Walidacja odbywa się przed wykonaniem logiki biznesowej, co zapobiega zapisywaniu niepoprawnych danych do bazy.

---

## 9. Konfiguracja środowisk (Development / Production)

Aplikacja obsługuje różne ustawienia konfiguracyjne w zależności od środowiska uruchomieniowego:

* **Development** – lokalne uruchomienie, szczegółowe komunikaty błędów
* **Production** – środowisko produkcyjne, zwiększone bezpieczeństwo

Wykorzystywana jest zmienna środowiskowa:

```
ASPNETCORE_ENVIRONMENT
```

Dzięki temu aplikacja automatycznie wybiera odpowiednie ustawienia bez konieczności modyfikacji kodu.

---

## 10. Connection pooling i dostęp do bazy danych

Połączenie z bazą PostgreSQL realizowane jest za pomocą **Entity Framework Core** oraz provider’a **Npgsql**.

Zastosowano **connection pooling** w celu poprawy wydajności i skalowalności aplikacji:

* `AddDbContextPool`
* jawna konfiguracja minimalnej i maksymalnej liczby połączeń
* obsługa retry policy (`EnableRetryOnFailure`)

Connection stringi przechowywane są w zmiennych środowiskowych (np. plik `.env`), co zwiększa bezpieczeństwo i umożliwia łatwą konfigurację różnych środowisk.

---

## 11. Konfiguracja CORS

W projekcie zastosowano poprawną konfigurację **CORS (Cross-Origin Resource Sharing)**, zależną od środowiska:

* **Development** – dostęp tylko z `http://localhost:3000`
* **Production** – dostęp wyłącznie z domeny frontendu aplikacji

Nie stosowano `AllowAnyOrigin`, co zapobiega nieautoryzowanemu dostępowi do API.

---

## 12. Bezpieczeństwo

* brak wrażliwych danych w repozytorium
* hasła użytkowników przechowywane w postaci haszowanej (BCrypt)
* kontrola dostępu po stronie backendu
* oddzielenie konfiguracji środowisk
* walidacja wszystkich danych wejściowych
* ochrona przed atakami typu SQL injection poprzez EF Core

---

## 13. Testowanie

### Uruchamianie testów jednostkowych
```bash
dotnet test
```

### Pokrycie testami
Projekt zawiera testy jednostkowe dla:
- Operacji na użytkownikach (GraphQL queries)
- Mutacji autoryzacji
- Walidacji modeli

---

## 14. Wdrożenie produkcyjne

### Wymagania serwerowe:
- Serwer z .NET 8.0 runtime
- PostgreSQL 12+
- Nginx/Apache jako reverse proxy
- SSL certificate (Let's Encrypt)

### Kroki wdrożenia:
1. Skonfiguruj zmienne środowiskowe produkcyjne
2. Uruchom migracje bazy danych
3. Skonfiguruj reverse proxy
4. Włącz SSL
5. Skonfiguruj monitoring i logowanie

---

## 15. Rozwiązywanie problemów

### Częste problemy:

**Problem**: `Connection refused` przy łączeniu z bazą danych
**Rozwiązanie**: Sprawdź connection string i czy PostgreSQL jest uruchomiony

**Problem**: `JWT token invalid`
**Rozwiązanie**: Upewnij się, że JWT_SECRET jest ustawiony i ma minimum 256 bitów

**Problem**: CORS errors
**Rozwiązanie**: Sprawdź konfigurację CORS w Program.cs

**Problem**: Migracje nie działają
**Rozwiązanie**: Upewnij się, że masz odpowiednie uprawnienia do bazy danych

---

## 16. Autor

Projekt wykonany w ramach zaliczenia przedmiotu.

Autor: *[Twoje imię i nazwisko]*
