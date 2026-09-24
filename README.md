# Lifestyle Checker

A C# ASP.NET Core Razor Pages application for the Lifestyle Checker interview task. The solution currently contains the starter web app and xUnit test project; patient lookup and scoring are the next build steps.

## Requirements

- .NET 10 SDK

## Run

From this directory:

```powershell
dotnet run --project src/LifestyleChecker/LifestyleChecker.csproj
```

Open the local URL printed by the command. The current page is the standard Razor Pages template.

## Test

```powershell
dotnet test LifestyleChecker.slnx
```

The test project is wired into the solution. Behaviour tests will be added alongside patient matching and scoring; there are no tests to discover yet.

The subscription key is not needed for the scaffold. When the API client is added, configure it with a local secret or environment variable. Never add the key to source control.
