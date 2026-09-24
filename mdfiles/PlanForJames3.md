# Step 3: Add the patient API client and Part One form

## Goal

Ask for the patient's NHS number, surname, and date of birth. Check those details against the supplied API, then show one of the four outcomes in the assignment brief. The age and matching rules from Step 2 should do the comparisons; this step connects them to the API and the web page.

Work in `src/LifestyleChecker`. The existing xUnit project is `tests/LifestyleChecker.Tests`.

## How this fits the current project

The project already has `Rules/PatientMatcher.cs` (`MyPatientMatcher` namespace), `Rules/AgeCalculator.cs` (`MyAgeCalculator` namespace), and an xUnit project. `Pages/Index.cshtml` and `Pages/Index.cshtml.cs` are still the template home page, so use them for the Part One form. `Api/PatientApiClient.cs` and `Models/PatientInfo.cs` are currently empty files. `Program.cs` now has an `AddHttpClient<PatientApiClient>` registration with a five-second timeout, the API base address, and configuration for the subscription key; finish the client class next so that registration has a real type to construct.

### A. Check the registration in `Program.cs`

Keep the registration after `builder.Services.AddRazorPages()` and add `using LifestyleChecker.Api;` at the top:

```csharp
builder.Services.AddHttpClient<PatientApiClient>(client =>
{
    client.BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/");
    client.Timeout = TimeSpan.FromSeconds(5);

    var key = builder.Configuration["PatientApi:SubscriptionKey"];
    if (string.IsNullOrWhiteSpace(key))
        throw new InvalidOperationException("Configure PatientApi:SubscriptionKey.");

    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
});
```

This registration is already present in the project. The `HttpClient` it configures will be passed into `PatientApiClient` automatically. The missing-key check runs when that typed client is created.

For local development, run these commands from `src/LifestyleChecker` (replace the placeholder with the supplied key):

```powershell
dotnet user-secrets init
dotnet user-secrets set "PatientApi:SubscriptionKey" "YOUR_KEY"
```

`dotnet user-secrets init` adds a user-secrets ID to the project file; it does not add the key to the repository. Alternatively, set an environment variable named `PatientApi__SubscriptionKey`. Add setup instructions to the app `README.md`, without including the real key.

### B. Fill in the API files

In `Models/PatientInfo.cs`, add a small model for the *parsed* record:

```csharp
namespace LifestyleChecker.Models;

public record PatientInfo(string NhsNumber, string Name, DateOnly Born);
```

In `Api/PatientApiClient.cs`, create a class whose constructor takes `HttpClient`:

```csharp
namespace LifestyleChecker.Api;

public class PatientApiClient(HttpClient httpClient)
{
    // Implement GetPatientAsync(string nhsNumber, CancellationToken cancellationToken).
}
```

Before sending a request, reject an NHS number that is empty or contains anything except digits. Do not convert it to an integer: the test API uses nine-digit sample values, and a string preserves leading zeroes. Send `GET tech-test/t2/patients/{nhsNumber}` using the configured base address. The subscription key is already supplied by the configured `HttpClient` header.

Make the method return a result that distinguishes three cases: a valid `PatientInfo`, `NotFound` for HTTP 404, and `Unavailable` for timeout, network failure, another HTTP status, invalid JSON, missing fields, or an invalid birth date. An enum plus a result record is one simple design. Do not use `null` for both 404 and API failures. Parse `born` with `DateOnly.TryParseExact(born, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)` after reading the JSON with `System.Text.Json`.

### C. Use `Index` as Part One

In `Pages/Index.cshtml.cs`, inject the typed client into the existing `IndexModel`:

```csharp
using LifestyleChecker.Api;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifestyleChecker.Pages;

public class IndexModel(PatientApiClient patientApiClient) : PageModel
{
    // Add bound form values, validation, and OnPostAsync here.
}
```

Add `using MyPatientMatcher;` and `using MyAgeCalculator;` when calling the existing rule classes. In `Pages/Index.cshtml`, replace the welcome template with a POST form for NHS number, surname, and date of birth. Use Razor tag helpers (`asp-for`, `asp-validation-for`, and `asp-validation-summary`), visible labels, and an antiforgery token. A date input posts `yyyy-MM-dd`; parse that format explicitly before calling the API. Keep the bound form values on validation errors.

The POST handler should validate the three inputs, call `GetPatientAsync` with the NHS number, handle 404 and service errors separately, then call `PatientMatcher.IsMatch(...)`. On a match, calculate `AgeCalculator.GetAge(patient.Born, DateOnly.FromDateTime(DateTime.Today))`. Show the brief's exact under-16 text or store the verified age in server-controlled session state and redirect to Part Two. Add the session middleware and guard both Part Two handlers when implementing Step 4.

### D. Build in this order

1. Finish `PatientInfo` and `PatientApiClient` so the existing `Program.cs` registration has a concrete client.
2. Change `Index.cshtml.cs` to accept the client and process the form.
3. Replace `Index.cshtml` with the labelled form and validation feedback.
4. Add fake HTTP-handler and page-flow tests described below, then document key setup in `README.md`.

## 1. Configure the patient API client

Create `src/LifestyleChecker/Api/PatientApiClient.cs` and a small response model, such as `src/LifestyleChecker/Models/PatientInfo.cs`.

- Register the client with `AddHttpClient` in `Program.cs` and inject it into the Part One page model. Set a short timeout.
- Use the actual endpoint: `GET https://al-tech-test-apim.azure-api.net/tech-test/t2/patients/{nhsNumber}`. Only the NHS number belongs in this API path; the surname and date of birth are checked after the response arrives.
- Send the key in the `Ocp-Apim-Subscription-Key` request header. Read it from an environment variable or .NET user secrets. Do not put a real key in source code, `appsettings.json`, browser code, or committed files. Document how to configure it in the app README.
- Keep the NHS number as a string. The brief's sample numbers have nine digits; do not impose a ten-digit checksum rule on this test API. Ensure the value is non-empty and consists only of digits before making the request.
- The JSON fields are `nhsNumber`, `name`, and `born`. The example name is `DOE, John`; `born` is a string in `dd-MM-yyyy` format. Use `System.Text.Json` to deserialize and `DateOnly.TryParseExact` with `CultureInfo.InvariantCulture` to parse the date. Treat missing fields, an invalid date, and malformed JSON as an unavailable or invalid API response.

Give the caller distinct results for:

| API result | Meaning for the page |
| --- | --- |
| 200 with a valid patient record | Compare the submitted details with the record. |
| 404 | Show `Your details could not be found`. |
| Timeout, network failure, other HTTP error, or malformed response | Show a clear service unavailable message; do not claim the patient was not found. |

Do not catch every exception and turn it into the same error: the page must be able to distinguish a 404 from a service problem. Avoid logging the key or patient details, including the NHS number in the request path.

## 2. Build the Part One form

Add `Pages/PartOne.cshtml` and `Pages/PartOne.cshtml.cs` (or use the existing index page for this form). Ask for all three required fields:

1. NHS number
2. Surname
3. Date of birth

Use a POST form with ASP.NET Core antiforgery protection. Give each field a visible label, show validation errors beside the field and in an accessible summary, and keep entered values when correcting validation errors. Validate required fields and the date before calling the API.

On a valid submission, process the result in this order:

1. Call the API using the entered NHS number.
2. If it returns 404, show `Your details could not be found`.
3. If the API fails or returns an unusable record, show the service unavailable message.
4. Use `PatientMatcher` to compare the entered NHS number, surname, and date of birth with the returned record. The matcher should compare the surname before the comma in `name`, ignore surrounding whitespace and case in the surname, and require exact NHS number and date matches. If any detail differs, show `Your details could not be found`.
5. Once all details match, calculate age from the **returned** date of birth using `AgeCalculator` and the current calendar date. If the patient is under 16, show exactly `You are not eligble for this service` (including the spelling in the brief).
6. Otherwise, allow the patient to proceed to Part Two. A separate success page is not required.

The four required outcomes are therefore:

| Condition | User outcome |
| --- | --- |
| Patient not found (404) | `Your details could not be found` |
| Patient found, details do not match | `Your details could not be found` |
| Details match, patient is under 16 | `You are not eligble for this service` |
| Details match, patient is 16 or older | Proceed to Part Two |

API failure is an additional error state, separate from the four patient outcomes.

## 3. Prepare the handoff to Part Two

Part Two must only accept someone who passed Part One. Pass that fact, and the age needed for scoring, through a server-controlled mechanism. Do not accept an age, eligibility flag, or precomputed score supplied by the browser. Guard both the Part Two GET and POST handlers. Do not put patient details in browser URLs, store them permanently, or log them. Finish the guarded questionnaire and result flow in Step 4.

## 4. Check the behaviour

Add focused API-client tests with a fake HTTP handler and page or integration tests with a fake patient API client. Cover:

- Valid record and exact match, including surname case and surrounding whitespace.
- 404 and each kind of details mismatch, all showing the brief's same not-found text.
- An under-16 patient and a patient on their 16th birthday.
- Timeout, another HTTP error, malformed JSON, and an invalid `born` value, all showing a service error.
- Missing form fields and an invalid date, without making an API request.

Use controlled dates in age tests. The example patients' birth years change over time to keep their listed ages fixed, so do not hard-code their birth years. Automated tests should not need a real subscription key. If the supplied API and key are available, make one manual end-to-end check.

## When Step 3 is done

- The form collects all three required details and gives accessible validation feedback.
- The API client uses the correct endpoint and header without committing the key.
- All four outcomes show the required text or progress to Part Two, and service errors are distinct.
- The page reuses the Step 2 matching and age rules, with focused tests for the API and Part One flow.
