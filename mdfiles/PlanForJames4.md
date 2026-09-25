# Step 4: Add the question form, guarded flow, scoring, and result messages

## What you are building

After a patient passes the details check on the home page, they should see three required yes/no questions. Submitting all three answers should calculate a score using the **verified age from Part One**, then show one of the assignment's two exact messages. Someone who has not passed Part One must not be able to use the question form or open a result directly.

Work from the `JHallGit2/TechTestJames` solution folder. This step finishes Part Two; Step 5 can add broader integration and accessibility checks.

## What is already in this project

- `src/LifestyleChecker/Pages/Index.cshtml.cs` checks patient details. On success it writes `PartOnePassed = "true"` and `VerifiedAge = age` to `HttpContext.Session`, then redirects to `/PartTwo`. That page does not exist yet.
- `src/LifestyleChecker/Program.cs` already calls `AddDistributedMemoryCache()`, `AddSession()`, and `UseSession()`. You do not need to add session setup again. Session data is kept on the server; the browser gets a session cookie.
- `src/LifestyleChecker/Rules/RiskScorer.cs` already exposes `RiskScorer.CalculateRisk(int age, bool q1Yes, bool q2Yes, bool q3Yes)` in the `MyRiskScorer` namespace. Reuse it; do not duplicate the scoring chart in a page handler.
- `tests/LifestyleChecker.Tests/RiskScorerTests.cs` already tests many scores and boundaries. `tests/LifestyleChecker.Tests/Pages/IndexModelTests.cs` has a small `TestSession` you can reuse for page-model tests.

### The finished flow

```text
Index GET/POST (patient details)
    └── valid matching patient aged 16+ → session stores pass + verified age
                                         → PartTwo GET (questions)
                                         → PartTwo POST (validate and score)
                                         → session stores result category
                                         → Result GET (exact message)
```

Keep the form answers in the request only. Do not place NHS number, surname, date of birth, age, answers, or score in a query string or hidden field. In this plan, the session stores only the Part One pass flag, verified age, and final result category (`Low` or `High`).

## 1. Make the Part One handoff safe when someone starts again

The current `IndexModel.OnPostAsync` sets the session keys after a successful check. Add a small reset at the **start** of every new Part One POST, before validation and the API call:

```csharp
HttpContext.Session.Remove("PartOnePassed");
HttpContext.Session.Remove("VerifiedAge");
HttpContext.Session.Remove("ResultCategory");
```

Otherwise, a previous successful check could leave a valid age in the same browser session after a later failed attempt. Also remove these three keys in `IndexModel.OnGet()` when the person deliberately returns to the start page. The successful branch at the end of `OnPostAsync` will set the pass flag and age again. Keep the existing 404, mismatch, under-16, and API-unavailable behaviour as it is.

To avoid spelling a key differently in two pages, you may add `Models/FlowSessionKeys.cs` with `const string` values and use those constants in `IndexModel`, `PartTwoModel`, and `ResultModel`. For a small exercise, consistently using the exact strings above is also fine.

**Why the guard matters:** Form values and URLs come from the browser and can be changed by a user. The verified age must come from the earlier server-side check. The session pass flag and age must both be present before scoring; also reject an age below 16.

## 2. Add the question page model

Create `src/LifestyleChecker/Pages/PartTwo.cshtml.cs`. A Razor Page normally has a `.cshtml` view and a matching `.cshtml.cs` page model. Use the existing `LifestyleChecker.Pages` namespace and add `using MyRiskScorer;` to call the scorer.

Add three **nullable** `bool` properties, each marked `[BindProperty]` and `[Required]`:

```csharp
[BindProperty]
[Required(ErrorMessage = "Answer question 1.")]
public bool? Q1Yes { get; set; }
```

Repeat for `Q2Yes` and `Q3Yes`, changing the message number. `bool?` starts as `null` when no radio button is chosen. Plain `bool` would start as `false`, which would incorrectly make a missing answer look like a genuine “No”. `[Required]` accepts both `true` and `false` but rejects `null`. Model binding also reports malformed values such as `Q1Yes=maybe`.

Implement the handlers in this order:

1. `OnGet()` reads `HttpContext.Session.GetString("PartOnePassed")` and `GetInt32("VerifiedAge")`. If the flag is not `"true"`, the age is missing, or the age is below 16, return `RedirectToPage("/Index")`. Otherwise return `Page()`.
2. `OnPost()` repeats **the same guard before checking answers or calling `RiskScorer`**. A user can send a POST without first loading the page, so a GET-only guard is insufficient.
3. If `ModelState` is invalid, or any of `Q1Yes`, `Q2Yes`, `Q3Yes` is null, return `Page()`. The page should then show validation errors and keep any selected answers. Do not score an incomplete form.
4. Call `RiskScorer.CalculateRisk(verifiedAge, Q1Yes.Value, Q2Yes.Value, Q3Yes.Value)`. The order of booleans matters: Q3's **No** answer adds points inside the existing scorer.
5. Store only the resulting category: `"Low"` when `score <= 3`, otherwise `"High"`. `HttpContext.Session.SetString("ResultCategory", category)` is enough. Remove `PartOnePassed` and `VerifiedAge` after setting the category, so the same verification cannot be used for another submission. Then `RedirectToPage("/Result")`.

For example, the key part of the POST handler can look like this (add your class, properties, and guard around it):

```csharp
if (!ModelState.IsValid || Q1Yes is null || Q2Yes is null || Q3Yes is null)
    return Page();

int score = RiskScorer.CalculateRisk(
    verifiedAge, Q1Yes.Value, Q2Yes.Value, Q3Yes.Value);

HttpContext.Session.SetString("ResultCategory", score <= 3 ? "Low" : "High");
HttpContext.Session.Remove("PartOnePassed");
HttpContext.Session.Remove("VerifiedAge");
return RedirectToPage("/Result");
```

Here `verifiedAge` is the integer you already read from session in the guard. Do not add an `[BindProperty]` for age, a hidden age input, or a score input. The page model should derive the category itself.

## 3. Add an accessible question form

Create `src/LifestyleChecker/Pages/PartTwo.cshtml` with `@page` and `@model PartTwoModel`. Use a `<form method="post">`, a validation summary, and a submit button. The ASP.NET Core form tag helper in this project supplies the antiforgery token for a normal POST form; leave tag helpers enabled through the existing `_ViewImports.cshtml`.

For each question, use one `<fieldset>` with its question in a `<legend>`. Inside it, use two radio inputs bound to the same nullable property, one with `value="true"` and one with `value="false"`. Give the two inputs different IDs and label each one. For example:

```html
<fieldset class="mb-3">
    <legend>Do you drink on more than 2 days a week?</legend>
    <div>
        <input asp-for="Q1Yes" type="radio" value="true" id="q1-yes" />
        <label for="q1-yes">Yes</label>
    </div>
    <div>
        <input asp-for="Q1Yes" type="radio" value="false" id="q1-no" />
        <label for="q1-no">No</label>
    </div>
    <span asp-validation-for="Q1Yes" class="text-danger"></span>
</fieldset>
```

Use the same pattern for the exact other questions:

1. **Do you smoke?** (`Q2Yes`, `q2-yes`, `q2-no`)
2. **Do you exercise more than 1 hour per week?** (`Q3Yes`, `q3-yes`, `q3-no`)

Put `<div asp-validation-summary="All" class="text-danger" role="alert"></div>` above the questions. The fieldsets group each pair of choices for keyboard and screen-reader users. `asp-validation-for` shows the specific missing-answer error. Do not preselect an answer; otherwise the user could submit without making a choice. Server-side validation must remain in place even if browser validation is enabled.

Add a normal link to `/Index` labelled “Start again” if useful. Returning to the start page clears the current flow through the `IndexModel.OnGet()` change from section 1.

## 4. Add the guarded result page

Create `src/LifestyleChecker/Pages/Result.cshtml.cs` and `Result.cshtml`.

In `ResultModel.OnGet()`, read `ResultCategory` from session. Accept only `"Low"` or `"High"`; redirect to `/Index` if it is missing or has any other value. Set a public `OutcomeMessage` property based on the category. The page should render `@Model.OutcomeMessage` as ordinary text. Do not accept a message, category, or score from the URL or POST body.

Use these **exact** strings, including the apostrophe and the brief's `you quality` wording:

| Session category | Score | Exact result message |
| --- | --- | --- |
| `Low` | 3 or less | `Thank you for answering our questions, we don't need to see you at this time. Keep up the good work!` |
| `High` | 4 or more | `We think there are some simple things you could do to improve you quality of life, please phone to book an appointment` |

For example, `Result.cshtml` only needs a heading, a paragraph containing the message, and a link to start again. Keep `ResultCategory` in session while the result page is open so refreshing it still works. A new visit to `/Index` clears it. Because Part Two removes the pass flag and age after a successful submission, using the browser's Back button cannot resubmit the questionnaire without a new Part One check.

## 5. Check the scoring rule and its boundaries

The existing `RiskScorer` has the agreed non-overlapping age bands. Do not copy the numbers into `PartTwoModel`; confirm that the existing rule remains:

| Verified age | Q1 Yes adds | Q2 Yes adds | Q3 No adds |
| --- | ---: | ---: | ---: |
| 16–21 | 1 | 2 | 1 |
| 22–40 | 2 | 2 | 3 |
| 41–65 | 3 | 2 | 2 |
| 66+ | 3 | 3 | 1 |

The assignment chart says `64+`, which overlaps `41–65`. The project plan records the team's confirmation that the last band begins at **66**. The existing `RiskScorer` and README already reflect this. Ages 65 and 66 should take different rows. One easy threshold check is age 18: Q1 Yes, Q2 Yes, Q3 Yes gives **3 / Low**; changing only Q3 to No gives **4 / High**.

## 6. Add focused tests and try the flow manually

Add `tests/LifestyleChecker.Tests/Pages/PartTwoModelTests.cs` and, if you test the result handler separately, `ResultModelTests.cs`. The existing `TestSession` in `IndexModelTests.cs` can be moved into a shared test helper file so both test classes can use it. Set a `DefaultHttpContext` and assign a `PageContext`, as the existing page test does. When testing a page model directly, remember that ASP.NET Core's automatic model binding and `[Required]` validation do **not** run just because you call `OnPost()` as a C# method. Add a `ModelState` error yourself for that kind of unit test, or verify the real binding and validation in Step 5's HTTP integration tests.

| Test case | Expected behaviour |
| --- | --- |
| `/PartTwo` GET without pass flag or age | Redirects to `/Index`. |
| `/PartTwo` POST without pass flag or age, even with three supplied answers | Redirects to `/Index`; no result category is stored. |
| Stored age below 16 | Redirects to `/Index`; scoring is never called. |
| Valid session, one unanswered question | Stays on question page with an error; no result category is stored. Test each of the three questions. |
| Valid session, age 18, Yes/Yes/Yes | Stores `Low`; redirects to `/Result`. |
| Valid session, age 18, Yes/Yes/No | Stores `High`; redirects to `/Result`. |
| Successful questionnaire POST | Removes pass flag and age; another questionnaire POST cannot reuse them. |
| Result GET without a valid category | Redirects to `/Index`. |
| Result GET with `Low` or `High` | Displays the matching exact text. |

The scorer's existing unit tests cover the age groups; add a 64-year-old case if you want explicit coverage of 64, 65, and 66 together. Run tests from the solution folder:

```powershell
dotnet test LifestyleChecker.slnx
```

Then run the app:

```powershell
dotnet run --project src/LifestyleChecker/LifestyleChecker.csproj
```

If you have the separately supplied API key configured, manually check: valid adult details reach the questions; leaving any question unanswered shows an error; scores 3 and 4 show their respective messages; opening `/PartTwo` or `/Result` in a fresh browser session redirects to the start. The sample patients' **birth years change**, so use the date of birth returned by the API rather than guessing a fixed year. Tests should use fake data and never require the real key.

## Step 4 is done when

- All three questions appear with distinct Yes/No options, labels, and validation feedback.
- Both `/PartTwo` GET and POST require the server-side Part One pass flag and verified age.
- A complete submission calls the existing scorer once and selects the result at the 3/4 boundary.
- The result route shows the exact assignment text, with no browser-supplied age or score.
- Starting a new Part One attempt clears earlier flow state, and a successful Part Two submission cannot be replayed from the same verification.
- `dotnet test LifestyleChecker.slnx` passes, and the manual flow works with a configured API key when one is available.
