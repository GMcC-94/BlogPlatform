# BlogPlatform

An ASP.NET Core MVC blog platform built as part of a Level 5 Software and Cloud Development assignment at Belfast Metropolitan College.

## Project Overview

A web application that allows users to register, log in, create blog posts, and comment on posts.

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server (LocalDB for development)
- ASP.NET Core Identity
- Bootstrap 5
- xUnit + Moq (testing)

## Project Structure

```
BlogPlatform/
├── Controllers/        # MVC Controllers
├── Models/             # Entity models (Post, Comment)
├── Data/               # DbContext and migrations
├── Views/              # Razor views
├── wwwroot/            # Static assets
BlogPlatform.Tests/
├── PostsControllerTests.cs  # Unit tests
```

## Setup Instructions

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (included with Visual Studio)

### Running Locally

1. Clone the repository
2. Navigate to the project folder:
   ```bash
   cd BlogPlatform
   ```
3. Apply migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```
5. Navigate to `https://localhost:7035` in your browser

### Running Tests

```bash
dotnet test
```

## Features

### Completed
- [x] Project setup with ASP.NET Core MVC
- [x] ASP.NET Core Identity (registration, login, logout)
- [x] Post model with EF Core migration
- [x] Comment model with EF Core migration
- [x] Posts Index endpoint — list all posts ordered by date
- [x] Posts Details endpoint — view a single post with comments
- [x] Posts Create endpoint — authenticated users can create a post
- [x] Posts Edit endpoint — author can edit their own post
- [x] Posts Delete endpoint — author can delete their own post
- [x] Comment endpoint — authenticated users can comment on a post  
- [x] Edit unit tests
- [x] Delete unit tests
- [x] Comment unit tests
- [x] Posts Index view
- [x] Posts Details view (with comments section)
- [x] Posts Create view
- [x] Posts Edit view

### Todo — Backend
- [ ] Integration tests for all endpoints

### Todo — Frontend
- [ ] Posts Index view
- [ ] Posts Details view (with comments section)
- [ ] Posts Create view
- [ ] Posts Edit view
- [ ] Posts Delete confirmation view
- [ ] Navigation bar (login/logout status)
- [ ] Bootstrap styling throughout

### Todo — Documentation
- [ ] Document project structure
- [ ] Document authentication setup
- [ ] Document third-party packages
- [ ] Write up test cases

## Database Models

### Post
| Field | Type | Notes |
|-------|------|-------|
| Id | int | Primary key |
| Title | string | Required |
| Body | string | Required |
| CreatedAt | DateTime | Set server-side |
| ImagePath | string? | Optional |
| AuthorId | string | FK to AspNetUsers |

### Comment
| Field | Type | Notes |
|-------|------|-------|
| Id | int | Primary key |
| Body | string | Required |
| CreatedAt | DateTime | Set server-side |
| AuthorId | string | FK to AspNetUsers |
| PostId | int | FK to Posts |

## Testing Strategy

- **Unit tests** — each endpoint has unit tests covering happy path and edge cases
- **Postman** — each endpoint manually verified before unit tests are written
- **Integration tests** — to be added once all endpoints are complete
