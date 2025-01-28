# ReactiveProbes 
[![.NET](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/dotnet.yml/badge.svg)](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/dotnet.yml) [![codecov](https://codecov.io/github/shyam-s00/ReactiveProbes/branch/main/graph/badge.svg?token=DPNKRAR83E)](https://codecov.io/github/shyam-s00/ReactiveProbes)  [![NuGet](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/release.yml/badge.svg)](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/release.yml)   [![NuGet Repo](https://img.shields.io/badge/nuget-v1.1.0-blue?style=flat&logo=nuget)](https://www.nuget.org/packages/ReactiveProbes) 

An ASP\.NET startup probe built with reactive extensions\. It validates all registered health checks and marks the startup as ready\. It exposes a `/ready` endpoint that returns healthy or unhealthy for startup probes to mark the API as ready or not\.

## Features

- Validates all registered health checks\.
- Marks the startup as ready based on health checks\.
- Exposes a `/ready` endpoint for readiness probes\.
- Adds REST API health checks\.
- Adds SQL Server health checks using connection strings or EF Core DbContext\.
- Registers reactive health probes with `/start` and `/stop` endpoints\.

## Installation

To include ReactiveProbes in your project, you can use the following NuGet package:

```sh
dotnet add package ReactiveProbes
```

## Usage

#### Register the health checks in your ASP.NET web API.
Add ReactiveProbes to your startup:

```csharp
services.AddReactiveProbes();
```
Register the probes:
```csharp
app.RegisterReactiveStartupProbe();
```
The `/ready` endpoint will be available to check the readiness of your application.

#### Adding REST API Health Checks
To add a REST API health check:

```csharp
services.AddHealthChecks()
        .AddRestApiCheck("api-check", "https://api.example.com/health");
```

#### Adding SQL Server Health Checks
To add a SQL Server health check using a connection string from the configuration:

```csharp
services.AddHealthChecks()
        .AddSqlServerCheck("DefaultConnection");
```
To add a SQL Server health check using an EF Core DbContext:

```csharp
services.AddHealthChecks()
        .AddSqlServerCheck<MyDbContext>();
```

#### Registering Reactive Health Probes
To register reactive health probes with start and stop endpoints:

```csharp
app.RegisterReactiveHealthProbe();
```
This sets up the following endpoints:  
* `/health` to display the health check status
* `/health/stop` to stop the health checks
* `/health/start` to start the health checks

## Contributing

Feel free to submit issues, fork the repository and send pull requests!

## License

This project is licensed under the MIT License.

---
