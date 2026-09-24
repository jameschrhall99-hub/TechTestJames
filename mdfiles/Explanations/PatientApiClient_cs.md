# `PatientApiClient.cs` explained

This file contains the code that asks the patient API for a record using an NHS number. It turns the HTTP response into a small result that a Razor Page can use. The client only looks up and parses the record; the page still needs to compare the returned details with the person's surname and date of birth, then check their age.

The related files are:

- `src/LifestyleChecker/Program.cs`, which registers `PatientApiClient` with `AddHttpClient`, sets the API base address and five-second timeout, and adds the subscription key header from configuration.
- `src/LifestyleChecker/Models/PatientInfo.cs`, which defines the parsed record: `NhsNumber`, `Name`, and `Born` (`DateOnly`).

## Imports and namespace

`System.Globalization` supplies `CultureInfo.InvariantCulture` and `DateTimeStyles` for parsing the API's date format. `System.Net` supplies `HttpStatusCode.NotFound`. `System.Text.Json` supplies `JsonDocument` and `JsonValueKind`. `LifestyleChecker.Models` makes `PatientInfo` available. The namespace `LifestyleChecker.Api` groups the client and its result types.

## What the result means

`PatientLookupStatus` has three values:

| Status | Meaning |
| --- | --- |
| `Success` | A successful HTTP response contained a usable patient record. |
| `NotFound` | The API returned HTTP 404 for that NHS number. |
| `Unavailable` | The client could not obtain a usable record for another reason. |

`PatientLookupResult` is a record with a `Status` and an optional `Patient`. On success, `Patient` holds a `PatientInfo`. The code creates `NotFound` and `Unavailable` results without a patient, so the optional property defaults to `null`. Callers should inspect `Status` before using `Patient`.

The status is important for the Part One page: a 404 means it can show `Your details could not be found`; a timeout or bad API response needs a separate service unavailable message. A successful lookup does **not** mean the user's surname and date of birth match the record.

## How the client gets `HttpClient`

`PatientApiClient(HttpClient httpClient)` is a primary constructor. ASP.NET Core supplies the configured `HttpClient` when it creates this typed client through the `AddHttpClient<PatientApiClient>` registration in `Program.cs`. That configured client holds the base address, timeout, and `Ocp-Apim-Subscription-Key` header. This file does not contain the key.

## What `GetPatientAsync` does

The method takes `nhsNumber` as a `string`, preserving any leading zeroes. It returns `Task<PatientLookupResult>` because the HTTP request and response reading happen asynchronously. The optional `CancellationToken` lets a caller cancel the work.

1. **Check the input.** `IsNullOrWhiteSpace` rejects an empty value. The `Any` expression rejects every character outside ASCII `0` through `9`. For invalid input, the current method returns `Unavailable` before sending a request. The form should validate input itself so the user receives a field error instead of a service error.
2. **Send the GET request.** `GetAsync("tech-test/t2/patients/{nhsNumber}", cancellationToken)` combines that relative path with the base address in `Program.cs`, producing `https://al-tech-test-apim.azure-api.net/tech-test/t2/patients/{nhsNumber}`. Only the NHS number goes in the path. `using var response` disposes the HTTP response when the method leaves the `try` block.
3. **Interpret the HTTP status.** A 404 becomes `NotFound`. Any other non-2xx status becomes `Unavailable`. A 2xx response proceeds to JSON parsing.
4. **Read and parse JSON.** `ReadAsStringAsync` reads the response body. `JsonDocument.Parse` turns it into a JSON document, which is disposed by `using var document`. The root must be a JSON object. The code then looks for the exact property names `nhsNumber`, `name`, and `born`.
5. **Check the fields.** Each of those properties must contain a JSON string, and none of the resulting strings may be empty or whitespace. If a field is missing, has another JSON type, or is blank, the method returns `Unavailable`.
6. **Parse the birth date.** The API sends `born` as text such as `25-12-1990`. `DateOnly.TryParseExact` requires the `dd-MM-yyyy` format. `CultureInfo.InvariantCulture` keeps parsing independent of the computer's locale. An invalid date becomes `Unavailable`.
7. **Return the record.** A valid response becomes `new PatientInfo(returnedNhsNumber, name, date)`, wrapped in a `PatientLookupResult` with status `Success`.

## Error handling and cancellation

The three `catch` blocks turn expected service and data failures into `Unavailable`:

- `OperationCanceledException` is caught **only when the caller's token was not cancelled**. This covers a timeout from the configured `HttpClient`. If the caller cancelled the operation, that cancellation is allowed to propagate.
- `HttpRequestException` covers HTTP transport problems such as a connection failure.
- `JsonException` covers malformed JSON that `JsonDocument.Parse` cannot read.

The method does not catch every exception. Programming errors and caller cancellation therefore remain visible instead of being silently reported as an API outage. It also does not log the subscription key or patient details.

## What the Part One page still needs to do

After `Success`, use `PatientMatcher` to compare the entered NHS number, surname, and date of birth with `result.Patient`. Then calculate age from the **returned** `Born` date using `AgeCalculator`. The client deliberately does not make those decisions. `NotFound` and `Unavailable` must lead to different messages, as described in `mdfiles/PlanForJames3.md`.
