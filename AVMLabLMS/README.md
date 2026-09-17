# AVMLabLMS

AVMLabLMS is an ASP.NET Core MVC + Web API project created for the AVMLabs Technical Assessment. It includes full functionality for managing Clients, Work Orders, Invoices, Payments, and Reports, while accurately tracking NBL (No Balance Limit) status.

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or full instance)

## Project Structure

This is a single project architecture focusing on simplicity and clarity, suitable for an intermediate developer without over-engineering.

- **Controllers & Views**: Handles UI and Razor pages rendering.
- **ApiControllers**: Provides RESTful APIs endpoints for Javascript front-end fetch calls.
- **Services**: Encapsulates business rules (NBL Calculation, Gateway Fees).
- **Models & DTOs**: Entity framework models and data transfer objects.
- **SQL**: Contains the `Assignment.sql` script with required queries and Stored Procedures.

## Business Rules Implemented

1. **NBL Calculation**: NBL status means outstanding balance is 0. Outstanding balance = Pending Invoice Amount + In-Transit Work Order Amount. Credit Limit does NOT affect NBL status.
2. **Work Order Validation**: Only NBL clients can create Work Orders.
3. **Gateway Fee**: If Payment Mode is 'Online', a 2% fee is separately calculated and debited in the client's ledger.

## Setup Instructions

1. **Database Setup**
   The application uses EF Core Code-First migrations. Connection string is configured in `appsettings.json` to use localdb by default:
   ```json
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AVMLabLMSDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   ```

2. **Restore Packages & Build**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Apply Migrations**
   Install the EF tools if not already present:
   ```bash
   dotnet tool install --global dotnet-ef
   ```
   Then apply migrations (or let the app auto-migrate on startup):
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
   *Note: The application has a DbSeeder configured in Program.cs that automatically calls `context.Database.Migrate()` and seeds required data on startup.*

4. **Run the Application**
   ```bash
   dotnet run
   ```
   
   - **MVC Frontend**: Open `https://localhost:xxxx/` or `http://localhost:yyyy/`
   - **Swagger UI**: Open `/swagger/index.html` to view API documentation and test endpoints.

## Features

- Client listing with search and pagination (API).
- Client ledger showing running balance, gateway fees, and payments.
- Work Order creation with real-time NBL validation via fetch API.
- Dashboard with dynamic Chart.js integration showing 30-day revenue and Top 5 Outstanding Clients.
