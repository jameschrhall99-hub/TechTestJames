# `Index.cshtml.cs` explained

This file is the C# page model for the Razor Pages home page. It receives the three details entered in `Index.cshtml`, asks `PatientApiClient` to look up the NHS number, compares the returned record with the submitted details, checks the patient's age, and either displays an outcome or stores the Part One pass and redirects to Part Two.

The related files are:

- `src/LifestyleChecker/Pages/Index.cshtml`, which displays the form and renders validation and outcome messages.
- `src/LifestyleChecker/Api/PatientApiClient.cs`, which sends the API request and returns `Success`, `NotFound`, or `Unavailable`.
- `src/LifestyleChecker/Models/PatientInfo.cs`, which represents the parsed patient record.
- `src/LifestyleChecker/Rules/PatientMatcher.cs`, which compares the submitted NHS number, surname, and birth date with the returned record.
- `src/LifestyleChecker/Rules/AgeCalculator.cs`, which calculates age from a date of birth and a supplied current date.
- `src/LifestyleChecker/Program.cs`, which registers the typed API client and configures session middleware.

## Imports, namespace, and page model

`System.ComponentModel.DataAnnotations` provides `[Required]` and `[RegularExpression]` for input validation. `System.Globalization` provides invariant date parsing. `LifestyleChecker.Api` gives access to the client and its result/status types. The ASP.NET Core MVC namespaces provide model binding, page handlers, page responses, and redirects. The last two imports refer to the existing matching and age rules; their namespaces are `MyPatientMatcher` and `MyAgeCalculator`.

`IndexModel(PatientApiClient patientApiClient)` uses a primary constructor. ASP.NET Core dependency injection supplies the `PatientApiClient` registered in `Program.cs`. The model can therefore call the API without constructing an `HttpClient` itself.

## Messages and bound form values

`NotFoundMessage` and `UnderAgeMessage` are constants so the required outcome text is kept in one place. The spelling `eligble` in the under-age message follows the assignment brief.

Each form property has `[BindProperty]`, which tells Razor Pages to copy the submitted form value onto the property before calling the POST handler. The page can then use the same properties to repopulate the inputs when it returns `Page()` after a validation error.

- `NhsNumber` is a string so leading zeroes are preserved. `[Required]` rejects a missing value, and `[RegularExpression]` accepts digits only.
- `Surname` is required. Trimming and case-insensitive comparison are handled later by `PatientMatcher`.
- `DateOfBirth` is kept as a string so the posted date can be parsed explicitly in the format used by an HTML date input.
- `OutcomeMessage` is not bound from the browser. The handler sets it on the server for a result such as not found, under age, or service unavailable. The Razor page renders it as a status message.

## GET handler

`OnGet()` handles a normal request to the page. It is empty because the form properties already have empty initial values and no lookup should happen until the user submits the form.

## POST handler: validate before looking up

`OnPostAsync(CancellationToken cancellationToken)` handles the form submission asynchronously. ASP.NET Core validates the data annotations during model binding. The explicit `DateOnly.TryParseExact` call then parses the submitted date using `yyyy-MM-dd` and `CultureInfo.InvariantCulture`, so parsing does not depend on the server's language or date settings.

If parsing fails, the handler adds a field error to `DateOfBirth`. It then checks `ModelState.IsValid`. When the form has an error, `return Page()` renders the same page with its field values and validation feedback, and the API is not called.

The date is checked for valid format here. A future date is syntactically valid but is not explicitly rejected before the lookup; `AgeCalculator` throws for a future birth date. If future dates need a friendly field-level error, add a check against today's date before calling the API.

## API lookup and outcomes

After validation, `patientApiClient.GetPatientAsync(NhsNumber, cancellationToken)` performs the lookup. Passing the cancellation token allows a disconnected or cancelled request to stop the API work.

The handler processes the result in this order:

1. **`NotFound`:** set `OutcomeMessage` to `Your details could not be found`, then render the page.
2. **`Unavailable` or missing patient record:** show a separate message saying the patient service is unavailable. This avoids telling the person that their details are wrong when the API may simply be down.
3. **`Success`:** the record must be present. The handler creates a `PatientMatcher` and compares the submitted NHS number, surname, and parsed date with the API's NHS number, name, and parsed birth date. The matcher handles surname trimming and case-insensitive comparison, and checks the NHS number and date.
4. **Mismatch:** show the same `Your details could not be found` text used for a 404.

The successful API response only confirms that a record was returned. The submitted details still need to match that record before the page treats the patient as verified.

## Age and Part Two handoff

After all details match, the handler uses the patient's **returned** birth date, not the submitted text, to calculate age. It supplies today's calendar date to `AgeCalculator.GetAge`.

If the patient is younger than 16, the page shows the exact under-age message. Otherwise, the handler stores two values in the server-side session:

- `PartOnePassed` is set to `"true"` to record that Part One succeeded.
- `VerifiedAge` stores the age calculated from the API record for Part Two scoring.

The code does not put the patient's name, NHS number, or date of birth into session. `Program.cs` registers the in-memory session services and calls `UseSession()` in the request pipeline. Part Two must read these session values and guard both its GET and POST handlers before displaying questions or calculating a score.

The handler redirects to `/PartTwo`. That page is planned for Step 4; until it exists, an eligible submission will redirect to a route that has no page yet.

## Overall request flow

```text
GET /                 -> display the form
POST / with errors    -> display validation feedback; do not call the API
POST / with valid data -> API lookup
  404                 -> show not-found message
  unavailable         -> show service error
  record              -> compare details
    mismatch          -> show not-found message
    match, age < 16   -> show under-age message
    match, age >= 16  -> store pass and age in session; redirect to Part Two
```
