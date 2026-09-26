# Lifestyle Checker

A C# ASP.NET Core Razor Pages application for the Lifestyle Checker interview task.
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

The test project is wired into the solution.

## Assumptions

- The confirmed scoring age bands are 16–21, 22–40, 41–65, and 66+. Someone aged 65 is in the 41–65 band.
- A 29 February birthday is treated as occurring on 1 March in non-leap years.
- "High" message changed from "improve you quality of life" to "improve your quality of life"
