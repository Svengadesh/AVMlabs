# AVMLabLMS

AVMLabLMS is an ASP.NET Core MVC + Web API application created for the AVMLabs Technical Assessment.

The application manages Clients, Work Orders, Invoices, Payments and Reports. It also implements NBL (No Balance Limit) validation and client ledger tracking.

## Prerequisites

- .NET 10 SDK
- Visual Studio Code or Visual Studio
- SQLite (no separate database server installation is required)

## Project Structure

This solution uses a single ASP.NET Core project for both MVC pages and Web API endpoints.

- **Controllers** - MVC controllers used to load Razor views.
- **ApiControllers** - REST API controllers used by the JavaScript/fetch frontend.
- **Services** - Contains business logic such as NBL calculation, Work Order validation, payments and reports.
- **Models** - Entity Framework Core database entities.
- **DTOs** - Data Transfer Objects used between the API and frontend.
- **Data** - DbContext and database seeding.
- **Views** - Razor/CSHTML pages.
- **wwwroot/css** - Application styling.
- **SQL** - Contains `Assignment.sql` with the SQL Server schema, seed data, queries and stored procedure.

## Database

The current application uses SQLite with Entity Framework Core. No SQL Server installation is required to run this application locally.

The database connection is configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=app.db"
}