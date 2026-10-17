# RaceDay API - Part 2 (RESTful API Development)

This is the complete, working backend API for the RaceDay event management system. It was built using ASP.NET Core, Entity Framework Core (Code-First), and SQL Server, and secured with JWT role-based authentication.

## 🛠️ Tech Stack
- **Framework:** ASP.NET Core Web API (.NET 10)
- **Database:** SQL Server (LocalDB) with EF Core Code-First Migrations
- **Authentication:** JWT Bearer Tokens & BCrypt password hashing
- **Documentation:** Swagger / OpenAPI
- **Testing:** xUnit, FluentAssertions, and WebApplicationFactory
- **CI/CD:** GitHub Actions

## 👥 The Two Roles
The system enforces strict role-based access control:
1. **Organiser:** Can manage events (CRUD), manage categories, view all users, view enrolments for their events, and capture race results.
2. **Participant:** Can view events, enrol in events, cancel their own enrolment, view their own enrolments, and view their own results.

## 🚀 Setup & Run Instructions

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (comes installed with Visual Studio)
- Visual Studio 2022

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/Nicola196/RaceDay.Api.git
   cd RaceDay.Api
