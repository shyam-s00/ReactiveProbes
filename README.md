# ReactiveProbes 
[![.NET](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/dotnet.yml/badge.svg)](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/dotnet.yml)  [![NuGet](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/release.yml/badge.svg)](https://github.com/shyam-s00/ReactiveProbes/actions/workflows/release.yml)


An ASP.NET startup probe built with reactive extensions. It validates all registered health checks and marks the startup as ready. It exposes a `/ready` endpoint that returns healthy or unhealthy for startup probes to mark the API as ready or not.

## Features

- Validates all registered health checks.
- Marks the startup as ready based on health checks.
- Exposes a `/ready` endpoint for readiness probes.

## Installation

To include ReactiveProbes in your project, you can use the following steps:

1. Clone the repository:
   ```sh
   git clone https://github.com/shyam-s00/ReactiveProbes.git
   ```
2. Open the project in your preferred IDE.

## Usage

1. Register the health checks in your ASP.NET application.
2. Add ReactiveProbes to your startup and register them.
3. The `/ready` endpoint will be available to check the readiness of your application.

## Contributing

Feel free to submit issues, fork the repository and send pull requests!

## License

This project is licensed under the MIT License.

---
