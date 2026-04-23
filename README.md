# BlogPlatform

An ASP.NET Core MVC blog platform built as part of a Level 5 Software and Cloud Development assignment at Belfast Metropolitan College.

## Project Overview

A web application that allows users to register, log in, create blog posts, comment on posts, and manage their own content.

## Tech Stack

| Technology | Purpose |
|------------|---------|
| ASP.NET Core MVC (.NET 8) | Web framework |
| Entity Framework Core 8 | ORM / database access |
| SQL Server (LocalDB) | Database (development) |
| ASP.NET Core Identity | User authentication |
| Bootswatch Cyborg + Bootstrap 5 | Frontend styling |
| Google Fonts — VT323 | Teletext-style typography |
| xUnit | Unit testing framework |
| Moq | Mocking library for unit tests |
| Microsoft.EntityFrameworkCore.InMemory | In-memory DB for unit tests |
| Microsoft.AspNetCore.Mvc.Testing | Integration test infrastructure |

## Project Structure

```
BlogPlatform/
├── Controllers/
│   ├── HomeController.cs         # Home page, recent posts
│   ├── PostsController.cs        # Post CRUD endpoints
│   └── CommentsController.cs     # Comment CRUD endpoints
├── Models/
│   ├── ApplicationUser.cs        # Extended Identity user with DisplayName
│   ├── Post.cs                   # Post entity
│   ├── Comment.cs                # Comment entity
│   └── ErrorViewModel.cs
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core DbContext
│   └── Migrations/               # EF Core migrations
├── Areas/
│   └── Identity/
│       └── Pages/
│           └── Account/
│               ├── Login.cshtml         # Scaffolded login page
│               └── Register.cshtml      # Scaffolded register page with DisplayName
├── Views/
│   ├── Home/
│   │   └── Index.cshtml          # Home page with 2 most recent posts
│   ├── Posts/
│   │   ├── Index.cshtml          # All posts list
│   │   ├── Details.cshtml        # Single post with comments
│   │   ├── Create.cshtml         # Create post form
│   │   ├── Edit.cshtml           # Edit post form
│   │   └── Delete.cshtml         # Delete confirmation
│   ├── Comments/
│   │   └── Edit.cshtml           # Edit comment form
│   └── Shared/
│       ├── _Layout.cshtml        # Teletext-themed layout
│       └── _LoginPartial.cshtml  # Login/logout navbar partial
├── wwwroot/                      # Static assets
└── Program.cs                    # App entry point and service registration

BlogPlatform.Tests/
├── PostsControllerTests.cs       # Unit tests for PostsController
├── CommentsControllerTests.cs    # Unit tests for CommentsController
└── IntegrationTests/             # Integration test infrastructure (see below)
```

## Setup Instructions

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (included with Visual Studio) or SQL Server Express

### Running Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/GMcC-94/BlogPlatform.git
   ```

2. Navigate to the project folder:
   ```bash
   cd BlogPlatform/BlogPlatform
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
cd BlogPlatform
dotnet test
```

To run with detailed output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

To run only unit tests for a specific controller:
```bash
dotnet test --filter "FullyQualifiedName~PostsControllerTests"
dotnet test --filter "FullyQualifiedName~CommentsControllerTests"
```

## User Authentication

Authentication is handled by **ASP.NET Core Identity**, the .NET equivalent of Django's built-in authentication system.

### How It Works

1. Users register with an email, password, and display name
2. Passwords are hashed automatically by Identity
3. On login, Identity validates credentials and issues a session cookie
4. The `[Authorize]` attribute restricts endpoints to authenticated users
5. Author-only actions (edit, delete) compare the current user's ID from claims against the post/comment `AuthorId`

### ApplicationUser

The default `IdentityUser` was extended with a custom `ApplicationUser` class:

```csharp
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
```

This allows a display name to be shown on posts and comments instead of exposing the user's email address.

### User Permissions

| Action | Anonymous | Authenticated | Author Only |
|--------|-----------|---------------|-------------|
| View posts | ✅ | ✅ | ✅ |
| View post detail | ✅ | ✅ | ✅ |
| Create post | ❌ | ✅ | — |
| Edit post | ❌ | ❌ | ✅ |
| Delete post | ❌ | ❌ | ✅ |
| Comment | ❌ | ✅ | — |
| Edit comment | ❌ | ❌ | ✅ |
| Delete comment | ❌ | ❌ | ✅ |

## Database Models

### ApplicationUser
Extends ASP.NET Core Identity's `IdentityUser`.

| Field | Type | Notes |
|-------|------|-------|
| Id | string | Primary key (GUID) |
| Email | string | Login credential |
| DisplayName | string | Shown on posts/comments |
| PasswordHash | string | Managed by Identity |

### Post
| Field | Type | Notes |
|-------|------|-------|
| Id | int | Primary key |
| Title | string | Required |
| Body | string | Required |
| CreatedAt | DateTime | Set server-side on create |
| ImagePath | string? | Optional — reserved for future use |
| AuthorId | string | FK to AspNetUsers |

### Comment
| Field | Type | Notes |
|-------|------|-------|
| Id | int | Primary key |
| Body | string | Required |
| CreatedAt | DateTime | Set server-side on create |
| AuthorId | string | FK to AspNetUsers |
| PostId | int | FK to Posts (NoAction on delete) |

## Third-Party Packages

### Main Project
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.24 | Identity with EF Core |
| Microsoft.AspNetCore.Identity.UI | 8.0.24 | Scaffolded Identity UI pages |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.24 | SQL Server EF Core provider |
| Microsoft.EntityFrameworkCore.Tools | 8.0.24 | Migrations CLI tooling |
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 8.0.24 | EF error page in development |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 8.0.0 | Identity scaffolding |

### Test Project
| Package | Version | Purpose |
|---------|---------|---------|
| xunit | 2.9.2 | Unit testing framework |
| Moq | 4.20.72 | Mocking library |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.0 | In-memory DB for unit tests |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | Integration test infrastructure |
| Microsoft.NET.Test.Sdk | 17.12.0 | Test runner |
| coverlet.collector | 6.0.2 | Code coverage |

## Testing Strategy

### Unit Tests
Each controller endpoint has dedicated unit tests covering:
- Happy path (correct data, correct user)
- Not found (invalid ID)
- Forbidden (wrong user attempting author-only action)
- Validation errors (empty fields)
- Data persistence (verify saves/updates/deletes hit the DB)

Unit tests use an **in-memory database** and a **mocked HttpContext** with a `ClaimsPrincipal` to simulate authenticated users without running the full HTTP pipeline.

### Postman Testing
Each endpoint was manually verified in Postman before unit tests were written, using a session cookie extracted from the browser after login.

### Integration Tests — Scoped Out

Full end-to-end integration tests were scoped out due to the complexity of handling **antiforgery tokens** in a test environment. ASP.NET Core generates a unique antiforgery token per request which must be extracted from the HTML response and re-submitted with every POST request. Implementing this correctly for full user flow testing (register → login → create post → comment) requires significant additional infrastructure.

Given more time, the following flows would be covered:

- Registration flow — POST to register with valid credentials, verify redirect
- Login flow — POST to login, verify session cookie returned
- Create post flow — authenticated POST, verify redirect to Index
- Edit post flow — authenticated POST, verify changes persisted in DB
- Delete post flow — authenticated POST, verify removed from DB
- Comment flow — authenticated POST, verify appears on Details page
- Authorisation flow — verify non-authors cannot edit or delete others' content

## Test Results

```
Test run for BlogPlatform.Tests.dll (.NETCoreApp,Version=v9.0)

Passed BlogPlatform.Tests.PostsControllerTests.DeleteConfirmed_ReturnsNotFound_WhenPostDoesNotExist
Passed BlogPlatform.Tests.CommentsControllerTests.Create_RedirectsToDetails_WithoutSaving_WhenBodyIsEmpty
Passed BlogPlatform.Tests.PostsControllerTests.Edit_Get_ReturnsForbid_WhenUserIsNotAuthor
Passed BlogPlatform.Tests.CommentsControllerTests.Delete_ReturnsNotFound_WhenCommentDoesNotExist
Passed BlogPlatform.Tests.CommentsControllerTests.Create_ReturnsNotFound_WhenPostDoesNotExist
Passed BlogPlatform.Tests.CommentsControllerTests.Edit_ReturnsNotFound_WhenCommentDoesNotExist
Passed BlogPlatform.Tests.PostsControllerTests.Delete_Get_ReturnsNotFound_WhenPostDoesNotExist
Passed BlogPlatform.Tests.CommentsControllerTests.Delete_DeletesCommentAndRedirectsToDetails_WhenUserIsAuthor
Passed BlogPlatform.Tests.CommentsControllerTests.Edit_ReturnsForbid_WhenUserIsNotAuthor
Passed BlogPlatform.Tests.PostsControllerTests.Delete_Get_ReturnsForbid_WhenUserIsNotAuthor
Passed BlogPlatform.Tests.CommentsControllerTests.Create_SavesCommentAndRedirectsToDetails_WhenValid
Passed BlogPlatform.Tests.CommentsControllerTests.Delete_ReturnsForbid_WhenUserIsNotAuthor
Passed BlogPlatform.Tests.CommentsControllerTests.Edit_UpdatesCommentAndRedirectsToDetails_WhenUserIsAuthor
Passed BlogPlatform.Tests.PostsControllerTests.Index_ReturnsViewResult_WithPostsOrderedByDateDescending
Passed BlogPlatform.Tests.PostsControllerTests.Edit_Post_ReturnsViewResult_WhenTitleOrBodyIsEmpty
Passed BlogPlatform.Tests.PostsControllerTests.Create_Post_ReturnsViewResult_WhenTitleOrBodyIsEmpty
Passed BlogPlatform.Tests.PostsControllerTests.Edit_Get_ReturnsViewResult_WhenUserIsAuthor
Passed BlogPlatform.Tests.PostsControllerTests.Details_ReturnsViewResult_WithPost_WhenPostExists
Passed BlogPlatform.Tests.PostsControllerTests.Create_Get_ReturnsViewResult
Passed BlogPlatform.Tests.PostsControllerTests.Edit_Post_ReturnsForbid_WhenUserIsNotAuthor
Passed BlogPlatform.Tests.PostsControllerTests.Edit_Get_ReturnsNotFound_WhenPostDoesNotExist
Passed BlogPlatform.Tests.PostsControllerTests.DeleteConfirmed_ReturnsForbid_WhenUserIsNotAuthor
Passed BlogPlatform.Tests.PostsControllerTests.Delete_Get_ReturnsViewResult_WhenUserIsAuthor
Passed BlogPlatform.Tests.PostsControllerTests.Edit_Post_UpdatesPostAndRedirectsToIndex_WhenValid
Passed BlogPlatform.Tests.PostsControllerTests.DeleteConfirmed_DeletesPostAndRedirectsToIndex_WhenUserIsAuthor
Passed BlogPlatform.Tests.PostsControllerTests.Details_ReturnsNotFound_WhenPostDoesNotExist

Total tests: 26 | Passed: 26 | Failed: 0 | Skipped: 0
Total time: 2.1433 Seconds
```

## Known Limitations

### Internet Connection Required
The application loads the Bootswatch Cyborg Bootstrap theme and the VT323 font directly from CDN links. An active internet connection is required for the styling to render correctly. In a production environment these would be served locally from `wwwroot/lib/` to remove this dependency.

### Email Confirmation Disabled
`RequireConfirmedAccount` is set to `false` in `Program.cs`. A full production application would require email confirmation on registration using a service. This was disabled for local development simplicity as no email provider is configured.

### Image Upload Reserved
The `ImagePath` field exists on the `Post` model and is included in the migration but image upload functionality is not yet implemented in the UI.

## Known Decisions and Bug Log

### GetCurrentUserId() refactor
**Issue:** `User.Identity!.Name` was not resolving correctly against the in-memory database during unit tests.

**Cause:** The `ClaimsPrincipal` set up in tests uses `ClaimTypes.Name` but `User.Identity.Name` resolves differently without the full ASP.NET authentication middleware present.

**Fix:** Replaced with `User.FindFirstValue(ClaimTypes.NameIdentifier)` which reads directly from the claims collection, consistent across both the full pipeline and mocked test contexts.

### Comment cascade delete
**Issue:** EF Core migration failed with `FK_Comments_Posts_PostId may cause cycles or multiple cascade paths`.

**Cause:** SQL Server does not allow multiple cascade delete paths to the same table. Both `Comment → Post` and `Comment → AspNetUsers` cascade to related tables.

**Fix:** Set `OnDelete(DeleteBehavior.NoAction)` on the `Comment → Post` relationship in `OnModelCreating`.

### Post delete FK constraint error
**Issue:** Deleting a post with comments from other users threw a SQL FK constraint error.

**Cause:** `OnDelete(DeleteBehavior.NoAction)` on the Comment → Post relationship prevents SQL Server from cascading the delete to comments automatically.

**Fix:** Explicitly load and delete all comments on the post before removing the post itself in `DeleteConfirmed`.

### ApplicationUser migration
**Issue:** EF could not create a `DbContext` when switching from `IdentityUser` to `ApplicationUser`.

**Cause:** Remaining `IdentityUser` references in `Post.cs` and `Program.cs` caused EF to see both types in the model simultaneously.

**Fix:** Replaced all `IdentityUser` references with `ApplicationUser` across models, controllers, Identity pages, and `Program.cs`.

## Resources

### ASP.NET Core & .NET
- [ASP.NET Core MVC docs](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview)
- [Entity Framework Core docs](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Identity docs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [dotnet CLI reference](https://learn.microsoft.com/en-us/dotnet/core/tools/)

### Frontend
- [Bootswatch Cyborg theme](https://bootswatch.com/cyborg/)
- [Bootstrap 5 docs](https://getbootstrap.com/docs/5.3/)
- [Google Fonts — VT323](https://fonts.google.com/specimen/VT323)
- [Bootstrap CDN via jsDelivr](https://www.jsdelivr.com/package/npm/bootstrap)

### Testing
- [xUnit docs](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Moq quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [EF Core InMemory testing](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database)
- [ASP.NET Core integration testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Handling antiforgery tokens in tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#antiforgery-support)
- [Mocking ClaimsPrincipal in tests](https://daninacan.com/how-to-mock-claimsprincipal-in-c/)
- [Controller unit tests with HttpContext](https://github.com/dotnet-labs/ControllerUnitTests)
- [Unit testing controllers with User dependency](https://rudirocha.medium.com/unit-testing-controllers-with-user-dependency-on-dotnet-core-ba7fc5fdd6f7)
- [Fake user for ASP.NET Core controller tests](https://gunnarpeipman.com/aspnet-core-test-controller-fake-user/)
