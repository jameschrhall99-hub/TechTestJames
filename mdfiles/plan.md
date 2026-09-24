# Lifestyle Checker — implementation plan

## Goal

Build a small C# web app that checks a patient's details against the supplied API, asks three lifestyle questions, calculates a risk score, and shows the specified result. Prioritise a complete, clear implementation of Parts One and Two. Treat configurable scoring (Part Three) as an optional extension.

## Suggested stack and structure

- ASP.NET Core Razor Pages for the two-step form flow and result page.
- A patient API client using IHttpClientFactory/HttpClient, with a short timeout.
- Separate classes for patient matching/eligibility and risk scoring so the rules can be tested without rendering web pages.
- xUnit tests for the business rules and key web flow behaviour.
- No database: the assignment does not require accounts or lasting patient records.

Possible project layout:

```text
JHallTest/
  LifestyleChecker.sln
  src/LifestyleChecker/            # Razor Pages, API client, scoring rules
  tests/LifestyleChecker.Tests/    # Unit and integration tests
  README.md                        # Run instructions, assumptions, test commands
```

Keep the design simple. Aim for readable names, small classes, and a working end-to-end flow before adding optional features.

## Part One: identify and check eligibility

1. Build a page with labelled NHS number, surname, and date-of-birth fields, required-field validation, and an accessible validation summary.
2. Keep the NHS number as a string so leading zeroes are preserved. The assignment's sample numbers have **nine digits**, so do not apply a real-world ten-digit NHS-number or checksum rule to this test API. Validate that the input is non-empty and contains digits.
3. Call `GET https://al-tech-test-apim.azure-api.net/tech-test/t2/patients/{nhsNumber}` from the server. Send the subscription key in the `Ocp-Apim-Subscription-Key` header. Use an environment variable or local .NET user secrets; never commit the key, print it in logs, or expose it in browser code.
4. Parse the API response fields `nhsNumber`, `name`, and `born`. The example uses `DOE, John` and `dd-MM-yyyy`. Parse dates explicitly with an invariant culture rather than relying on the machine's locale.
5. Compare the submitted NHS number and DOB with the returned record. Extract the surname before the comma, trim whitespace, and compare surnames without case sensitivity. Avoid accepting partial surname matches.
6. A 404 or any details mismatch must show exactly: “Your details could not be found”. Once details match, calculate age from the returned DOB as of the current date. If under 16, show exactly: “You are not eligble for this service” (the assignment spells “eligble” this way). Otherwise proceed to Part Two.
7. Handle an API timeout, service error, or malformed response gracefully with a message that clearly says the API/service is currently unavailable. Do not show “Your details could not be found” for these temporary connection or service problems; reserve that message for a 404 or details mismatch.

The API keeps its five sample patients at fixed *ages* by changing their birth year over time. Tests should use controlled dates or a fake clock. Do not hard-code the birth years from a lookup.

## Part Two: questions and scoring

Ask three required yes/no questions:

- Q1: Do you drink on more than 2 days a week?
- Q2: Do you smoke?
- Q3: Do you exercise more than 1 hour per week?

Score a “yes” for Q1 and Q2, and a “no” for Q3. Use the verified patient's age, calculated on the server. Do not accept an age or a precomputed score from the browser.

| Age band | Q1 yes | Q2 yes | Q3 no |
| --- | ---: | ---: | ---: |
| 16–21 | 1 | 2 | 1 |
| 22–40 | 2 | 2 | 3 |
| 41–65 | 3 | 2 | 2 |
| 66+ | 3 | 3 | 1 |

Show the assignment's exact outcome text:

- Score 3 or less: “Thank you for answering our questions, we don't need to see you at this time. Keep up the good work!”
- Score 4 or more: “We think there are some simple things you could do to improve you quality of life, please phone to book an appointment”

**Confirmed age boundaries:** The team confirmed that the final band is 66+, so age 65 belongs in 41–65. Use the non-overlapping bands shown above and cover ages 64, 65, and 66 in scoring tests. Record the confirmed boundary in the app README.

## Web flow and privacy

- Keep the fact that the patient passed Part One in server-side session state, or revalidate their details on the server before accepting Part Two. Do not let someone open the questionnaire/result route and submit an arbitrary age.
- Use standard POST forms with ASP.NET Core anti-forgery protection and validation. Make radio buttons and error messages usable with a keyboard and screen reader.
- Do not store patient data permanently. Avoid logging NHS numbers, names, DOBs, the API key, or answers. Do not include these values in URLs (apart from the NHS number required by the upstream API).
- Keep the UI small: patient details page → questions page → result page, plus the relevant error states. Use clear back/navigation behaviour without losing validation messages.

## Test checklist

- API returns 404; matching record; wrong surname; wrong DOB; under-16 patient; API failure/malformed response showing that the API/service is currently unavailable.
- Name parsing: case/whitespace differences and a surname mismatch.
- Age on a birthday, the day before a birthday, and the 16th birthday.
- Every question is required; an unauthorised direct POST to Part Two cannot calculate a score.
- Each scoring age band, all-no/all-yes answer sets, and scores exactly 3 and 4.
- Ages 64, 65, and 66, confirming that age 65 is in 41–65 and age 66 is in 66+.
- One manual end-to-end run with the supplied patient records and subscription key, if the API is available. Automated tests should use a fake API client and need no real key.

## Build order

1. Scaffold the Razor Pages app and test project; run both locally.
2. Implement and test the pure age, matching, and scoring rules.
3. Add the API client and Part One form with all four required outcomes.
4. Add the question form, guarded flow, scoring, and result messages.
5. Add a few focused integration tests and perform a manual accessibility/usability pass.
6. Write setup instructions: required .NET SDK, how to set the subscription key, how to run the app and tests, and any assumptions or known limitations.
7. If time remains, implement Part Three.

## Optional Part Three: change scoring without rebuilding

Read age bands, question points, and the result threshold from a JSON file stored outside the published application package (or another external configuration source). Validate at startup/reload that bands do not overlap, cover all eligible ages, and contain valid point values. A reloadable configuration mechanism can apply changes while the app runs. Keep the bundled default rules aligned with the agreed interpretation of the assignment chart. Document where the external file lives and how an operator changes it; merely editing an appsettings file inside a deployment would still require changing that deployment.

## Definition of done

A reviewer can clone the repository, configure their own subscription key without committing it, run the app and tests from the README, complete both parts, and see the required result for each outcome. The code clearly shows how age, matching, and scoring decisions are made.
