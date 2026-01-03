```md
# GroupFlow Backend

Backend aplikacji **GroupFlow** zrealizowany w technologii **ASP.NET Core** z wykorzystaniem **GraphQL (HotChocolate)** oraz **PostgreSQL**.

---

## 🧱 Technologie

- .NET 8
- ASP.NET Core
- GraphQL (HotChocolate)
- Entity Framework Core
- PostgreSQL
- Npgsql
- Docker 

---

## ⚙️ Wymagania

- .NET SDK 8.0
- PostgreSQL
- Zmienna środowiskowa `POSTGRES_CONN_STRING` i ' JWT_SECRET'

---

## 🔌 Konfiguracja

Aplikacja korzysta z połączenia z bazą danych PostgreSQL poprzez zmienną środowiskową env.

## 

Dodano indeksy na kluczach obcych oraz polach często filtrowanych i wyszukiwanych, co poprawia wydajność zapytań.