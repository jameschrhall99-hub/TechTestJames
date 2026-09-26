# Step 4 test plan: question form, session flow, and results

This is a guide for **adding tests**, not a record of tests already written. Work in `JHallGit2/TechTestJames`. The goal is to prove that only a verified patient can answer the questions, that all three answers are required, and that the correct result appears.

## First, know what is already covered

- `tests/LifestyleChecker.Tests/RiskScorerTests.cs` already checks the main scoring rules, including ages 65 and 66 and the 3/4 point example at age 18. Add an age **64** case if you want the complete 64/65/66 boundary explicitly covered. There is no need to duplicate every scoring combination in page-model tests.
- `tests/LifestyleChecker.Tests/Pages/IndexModelTests.cs` has one successful Part One POST test and an in-memory `TestSession` implementation.
- The test project already uses xUnit and references the application project. Add C# files under `tests/LifestyleChecker.Tests/Pages`; the SDK will include them automatically.
- The focused tests below do not need a real API key or a running web server. They call the page-model handlers directly.

## A few testing terms

- **Arrange:** create the page model, a fake session, and any answers or stored values.
- **Act:** call `OnGet()`, `OnPost()`, or `OnPostAsync(...)`.
- **Assert:** check the returned page/redirect **and** the session values that should have changed or stayed absent.
- `[Fact]` is one example. `[Theory]` with `[InlineData(...)]` runs the same test with several inputs.
- A `PageResult` means the handler stays on the current page. A `RedirectToPageResult` means it sends the browser to another Razor Page; check its `PageName`.

## 1. Reuse the fake session and set up page models

Create `tests/LifestyleChecker.Tests/Pages/PartTwoModelTests.cs` in the same `LifestyleChecker.Tests.Pages` namespace as `IndexModelTests.cs`. Because `TestSession` is currently `internal` in that namespace, the new test file can use it as it stands. If you prefer cleaner organisation, move the **whole** `TestSession` class to `tests/LifestyleChecker.Tests/Pages/TestSession.cs` and delete the original copy. Do not leave two classes with the same name.

Each page-model test needs a `DefaultHttpContext` with the test session attached, then a `PageContext`. For example:

```csharp
using LifestyleChecker.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifestyleChecker.Tests.Pages;

public class PartTwoModelTests
{
    private static (PartTwoModel Model, TestSession Session) CreateModel()
    {
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        return (model, session);
    }
}
```

`GetString`, `SetString`, `GetInt32`, and `SetInt32` are extension methods from `Microsoft.AspNetCore.Http`. Add that `using` to test files that call them. Give **each test** a fresh model and session so one test cannot affect another. To make a verified session, set both values:

```csharp
session.SetString("PartOnePassed", "true");
session.SetInt32("VerifiedAge", 18);
```

Do not set an age or pass flag from a browser field; these tests deliberately use server-side session state.

## 2. Test access to `/PartTwo`

Add these tests to `PartTwoModelTests.cs`. For every redirect, assert `Assert.IsType<RedirectToPageResult>(result)` and `Assert.Equal("/Index", redirect.PageName)`.

| Test | Arrange | Act | Assert |
| --- | --- | --- | --- |
| GET, empty session | No keys | `model.OnGet()` | Redirects to `/Index`. |
| GET, only pass flag | `PartOnePassed = "true"`; no age | `model.OnGet()` | Redirects to `/Index`. |
| GET, only age | `VerifiedAge = 18`; no flag | `model.OnGet()` | Redirects to `/Index`. |
| GET, too young | Pass flag and age 15 | `model.OnGet()` | Redirects to `/Index`. |
| GET, verified adult | Pass flag and age 18 | `model.OnGet()` | Returns `PageResult`. |
| POST, empty session | Set all three answers, but no session keys | `model.OnPost()` | Redirects to `/Index`; `ResultCategory` stays absent. |
| POST, incomplete session | Set all answers, but provide only one of the two required keys | `model.OnPost()` | Redirects to `/Index`; no result category. |
| POST, too young | Pass flag, age 15, all answers | `model.OnPost()` | Redirects to `/Index`; no result category. |

GET and POST both need guard tests: a person can send a POST without loading the form first. Use `Assert.Null(session.GetString("ResultCategory"))` to check that rejected submissions did not create a result.

## 3. Test required answers and invalid model state

Start each test with a valid pass flag and age. Test **each** missing answer separately: `Q1Yes = null`, `Q2Yes = null`, and `Q3Yes = null`, while the other two are set. Call `OnPost()`. It should return `PageResult`, leave `ResultCategory` absent, and keep the pass flag and age so the person can correct the form. A `[Theory]` taking a question number can reduce repeated setup, but three `[Fact]` tests are fine if they are clearer to you.

Also test a separate invalid `ModelState` case: set all three `bool?` answers, then call `model.ModelState.AddModelError(nameof(model.Q1Yes), "Invalid answer");`. `OnPost()` should return `PageResult` and not create a result. This proves that a binding error cannot be bypassed merely because the properties hold values.

**Important:** When you call `OnPost()` directly, ASP.NET Core does **not** perform model binding or `[Required]` validation for you. The handler's explicit null check makes the missing-answer unit tests useful, but they do not prove that the browser receives validation messages. The `ModelState.AddModelError(...)` test simulates an error; it does not test the real radio-button binding. Check that with the manual form pass below, or a later HTTP integration test.

## 4. Test scoring, category, and one-use verification

These two examples are enough to test the page's **3/4 result threshold** without repeating the full scorer suite:

| Verified age | Q1 | Q2 | Q3 | Score from existing scorer | Expected category |
| ---: | --- | --- | --- | ---: | --- |
| 18 | Yes | Yes | Yes | 3 | `Low` |
| 18 | Yes | Yes | No | 4 | `High` |

For each case, set the session pass flag and age, assign the three nullable properties, and call `OnPost()`. Assert all of the following:

1. It returns `RedirectToPageResult` with `PageName == "/Result"`.
2. `session.GetString("ResultCategory")` equals `Low` or `High` as expected.
3. `PartOnePassed` and `VerifiedAge` are gone: `GetString(...)` and `GetInt32(...)` return `null`.

Add one **replay** test: after a successful POST, call `OnPost()` again on the same model and session. The second call should redirect to `/Index`; the earlier result category should stay as it was. This verifies that the same Part One check cannot submit a second questionnaire. The page model has a static scorer call, so a unit test cannot directly count calls to it; checking the result and session changes is enough here.

## 5. Test `/Result`

Create `tests/LifestyleChecker.Tests/Pages/ResultModelTests.cs`. Set up `ResultModel` with `DefaultHttpContext`, `TestSession`, and `PageContext` in the same way as Part Two. Add these cases:

| Session category | Expected handler result | Expected `OutcomeMessage` |
| --- | --- | --- |
| Missing | Redirect to `/Index` | No message needed. |
| `Unexpected` | Redirect to `/Index` | No message needed. |
| `Low` | `PageResult` | `Thank you for answering our questions, we don't need to see you at this time. Keep up the good work!` |
| `High` | `PageResult` | `We think there are some simple things you could do to improve you quality of life, please phone to book an appointment` |

Use `Assert.Equal(expectedText, model.OutcomeMessage)` for the two successful cases. Keep the unusual `you quality` wording because it is the assignment's exact text. After a successful GET, also check that `ResultCategory` is still in session; call `OnGet()` a second time and verify it returns the same message. That covers a browser refresh.

## 6. Test starting Part One again clears old state

Add tests to the existing `IndexModelTests.cs`. Its constructor needs a `PatientApiClient`, so reuse the fake `HttpClient` setup already in that file, or extract a small factory method. `OnGet()` itself should make no API request.

1. **GET reset:** Put `PartOnePassed = "true"`, `VerifiedAge = 18`, and `ResultCategory = "Low"` into a fresh session. Call `IndexModel.OnGet()`. Assert all three keys are absent. **This test currently fails:** `IndexModel.OnGet()` is empty. Implement the three `Session.Remove(...)` calls described in `PlanForJames4.md` before expecting a green suite.
2. **POST reset on failure:** Put the same stale keys into a fresh session. Give the model invalid input (for example, add a `ModelState` error) and call `await OnPostAsync(CancellationToken.None)`. It should return `PageResult` and leave all three keys absent. Since the model is invalid, the API call is skipped. This covers the start-of-POST reset already present in the handler.

The existing successful POST test already checks that a matching adult stores the pass flag and verified age and redirects to `/PartTwo`; keep it.

## 7. Optional small scorer boundary addition

`RiskScorerTests.cs` already tests age 65 and age 66. Add age **64** with `Q1 = No`, `Q2 = No`, `Q3 = No`; expected score is **2**, from the 41–65 row. This makes the confirmed 64/65/66 boundary easy to show a reviewer. Do not rewrite the existing scorer tests just for this step.

## 8. Run and review

From the solution folder, run:

```powershell
dotnet test LifestyleChecker.slnx
```

Fix failing tests by checking the test setup first, then the handler. In particular, the new Index GET reset test should fail until that missing behaviour is implemented. Do not change an expected result merely to make a test pass.

For a manual form check, run the app with a separately configured subscription key and use a valid adult patient from the supplied API:

```powershell
dotnet run --project src/LifestyleChecker/LifestyleChecker.csproj
```

Check these browser paths: all three radio groups have separate Yes/No choices and labels; submit with one answer missing and confirm an error appears and earlier choices remain selected; complete a 3-point and 4-point case and confirm the exact message; try `/PartTwo` and `/Result` in a fresh browser session; press Back after a completed result and confirm another Part Two POST cannot reuse the old verification; use **Start again** and confirm the previous result is no longer accessible. The API changes sample birth years, so use its current returned date rather than a remembered year. Automated tests should keep using fake data and never require the real key.

## Completion checklist

- [ ] `PartTwoModelTests.cs` covers GET and POST guards, missing answers, invalid model state, both result categories, and replay.
- [ ] `ResultModelTests.cs` covers missing/invalid categories, both exact messages, and refresh.
- [ ] `IndexModelTests.cs` covers clearing old session state on GET and on a failed new POST.
- [ ] The optional age-64 scorer case is added if you want explicit 64/65/66 coverage.
- [ ] The test command passes, and the manual browser checks are completed when an API key is available.
