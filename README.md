# ReactiveProbes 
[![.NET](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/dotnet.yml/badge.svg)](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/dotnet.yml)  [![NuGet](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/release.yml/badge.svg)](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/release.yml)   [![NuGet Repo](https://img.shields.io/badge/nuget-v1.1.0-blue?style=flat&logo=nuget)](https://www.nuget.org/packages/ReactiveProbes)



An ASP.NET startup probe built with reactive extensions. It validates all registered health checks and marks the startup as ready. It exposes a `/ready` endpoint that returns healthy or unhealthy for startup probes to mark the API as ready or not.

## Features

- Validates all registered health checks.
- Marks the startup as ready based on health checks.
- Exposes a `/ready` endpoint for readiness probes.

## Installation

To include ReactiveProbes in your project, you can use the following NuGet package:

```sh
dotnet add package ReactiveProbes  
```


## Usage

1. Register the health checks in your ASP.NET web API.
2. Add ReactiveProbes to your startup:
   ```csharp
   services.AddReactiveProbes();
   ```
3. Register the probes:
   ```csharp
   app.RegisterReactiveStartupProbe();
   ```
4. The `/ready` endpoint will be available to check the readiness of your application.

## Contributing

Feel free to submit issues, fork the repository and send pull requests!

## License

This project is licensed under the MIT License.

---
