# start-microservices

## About

**start-microservices** is built on top of the [eshop-microservices](https://github.com/mehmetozkaya/eshop-microservices) baseline project. The eshop-microservices project provides a reference microservice architecture using .NET Aspire, including services for **Catalog**, **Basket**, **Ordering**, and a **WebApp** frontend. This repository forks that baseline and serves as the starting point for implementing various features such as AI integrations, advanced patterns, and more.

### Architecture Overview

| Service | Database | Description |
|---------|----------|-------------|
| **Catalog** | PostgreSQL | Product catalog management |
| **Basket** | Redis | Shopping cart / basket operations |
| **Ordering** | SQL Server | Order processing and management |
| **WebApp** | — | Frontend web application |
| **AppHost** | — | .NET Aspire orchestrator |

---

## EF Core Migrations

### Catalog Microservice (PostgreSQL)

#### 1. Install Required Packages

Add the following packages to the **Catalog** project:

```xml
<ItemGroup>
  <PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="13.1.2" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.5">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

#### 2. Create the Migration

1. Open **Package Manager Console** (PMC) in Visual Studio.
2. Right-click the **Catalog** project → **Set as Startup Project**.
3. Run the following command in PMC:

```powershell
Add-Migration InitialCreate -OutputDir Data/Migrations -Project Catalog
```

This generates migration files under `Data/Migrations/`, including an `InitialCreate.cs` file. Examine the `Up` and `Down` methods to verify the schema changes.

#### 3. Apply Migrations

There are two options to apply migrations:

1. **Manual** — Run `Update-Database` in PMC. This requires an additional step every time you run the app.
2. **Auto-Migrate on Startup** — Apply migrations automatically when the application starts using an extension method.

We follow the **second option**. See `Extensions.cs`:

```csharp
public static class Extensions
{
    public static void UseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        context.Database.Migrate();
        DataSeeder.Seed(context);
    }
}
```

Then call it in `Program.cs`:

```csharp
// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseMigration();

app.MapProductEndpoints();

app.Run();
```

### Ordering Microservice (SQL Server)

Follow the same process. In PMC:

```powershell
Add-Migration InitialCreate -OutputDir Data/Migrations -Project Ordering
```

The Ordering service also uses the same auto-migration pattern with its own `Extensions.cs` and `Program.cs`.

---

## Running the Application

### Prerequisites

- **Docker Desktop** must be installed and running (required by .NET Aspire for container orchestration of PostgreSQL, SQL Server, Redis, etc.).

### Steps

1. Set the **AppHost** project as the **Startup Project**.
2. Press **F5** or click **Run** in Visual Studio.
3. The Aspire dashboard will open, showing all services and their endpoints.

---

## Troubleshooting

### SQL Server Container Throws "Exited" Exception

If the SQL Server container exits immediately with logs similar to:

```
SQL Server 2022 will run as non-root by default.
This container is running as user mssql.
Your master database file is owned by mssql.
To learn more visit https://go.microsoft.com/fwlink/?linkid=2099216.
/opt/mssql/bin/sqlservr: The file archive [/opt/mssql/lib/sqlservr.sfp] is invalid. Please resolve and try again.
```

This is typically caused by **volume file corruption**.

**Solution:**

- Remove all related SQL Server volumes, prune containers and images, and re-run, **or**
- Comment out the `.WithDataVolume()` line in `AppHost.cs`:

```csharp
var sqlServer = builder
    .AddSqlServer("sqlserver")
    .WithDataVolume()  // if get exited error, comment this line
    .WithLifetime(ContainerLifetime.Persistent);
```
