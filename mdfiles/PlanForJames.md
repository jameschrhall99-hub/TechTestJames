# Step 2 guide for James: pure rules and tests

This step is about writing small C# methods that take values in and return values out. You do **not** need to build forms, call the patient API, or use the subscription key yet. Keeping these rules separate from Razor Pages makes them easier to reason about and test.

Work in `JHallTest`. The app project is `src/LifestyleChecker`; the xUnit project is `tests/LifestyleChecker.Tests`. The test project already references the app project.

## 1. Choose a small shape for the code

You can use these names, or sensible alternatives:

```text
src/LifestyleChecker/Rules/AgeCalculator.cs
src/LifestyleChecker/Rules/PatientMatcher.cs
src/LifestyleChecker/Rules/RiskScorer.cs
tests/LifestyleChecker.Tests/AgeCalculatorTests.cs
tests/LifestyleChecker.Tests/PatientMatcherTests.cs
tests/LifestyleChecker.Tests/RiskScorerTests.cs
```

Make the classes `public` so the test project can call them. A simple `static` class with a public method is fine for each rule. You do not need interfaces, dependency injection, or a database for this step.

Possible method shapes:

```csharp
int GetAge(DateOnly dateOfBirth, DateOnly today)
bool IsMatch(string enteredNhsNumber, string enteredSurname,
             DateOnly enteredDateOfBirth, string apiNhsNumber,
             string apiName, DateOnly apiDateOfBirth)
int Calculate(int age, bool drinksMoreThanTwoDays,
              bool smokes, bool exercisesMoreThanOneHour)
```

These are suggestions, not code you have to copy. Passing `today` into the age method keeps the result predictable in tests.

## 2. Implement age calculation first

- Start with `today.Year - dateOfBirth.Year`.
- Subtract one if the birthday has not happened yet this year.
- Decide what the method should do with a future date of birth (for example, throw `ArgumentOutOfRangeException`), and test that decision.
- Use `DateOnly` for calendar dates. Avoid `DateTime.Now` inside this rule.

Good first tests:

| DOB | Today | Expected age |
| --- | --- | ---: |
| 2008-09-24 | 2026-09-23 | 17 |
| 2008-09-24 | 2026-09-24 | 18 |
| 2010-09-23 | 2026-09-23 | 16 |
| 2010-09-24 | 2026-09-23 | 15 |

A leap-day birthday is a useful extra case. Choose and document how 29 February behaves in a non-leap year.

## 3. Implement patient matching

The API example returns `name` as `"DOE, John"`. Compare the text before the comma with the surname entered by the user. Trim surrounding spaces and compare without case sensitivity; do not accept partial matches. Compare NHS numbers exactly as **strings** and compare the two dates directly.

For example, entered surname `" doe "` should match `"DOE, John"`, while `"Do"` should not. If the API name has no comma or has an empty surname, return `false` rather than guessing.

Suggested test cases:

- Same NHS number, surname, and DOB -> match.
- Wrong NHS number -> no match.
- Wrong surname -> no match.
- Wrong DOB -> no match.
- Lowercase and extra surrounding spaces in surname -> match.
- Malformed API name -> no match.

The sample NHS numbers in the brief contain **nine digits**. Do not add a real-world ten-digit NHS checksum rule for this exercise. Parsing the API's `born` value (`dd-MM-yyyy`) is part of the API client in a later step; this matching rule can receive an already parsed `DateOnly`.

## 4. Implement scoring

For Q1 and Q2, add points when the answer is **yes**. For Q3, add points when the answer is **no**.

| Age | Q1 yes | Q2 yes | Q3 no |
| --- | ---: | ---: | ---: |
| 16-21 | 1 | 2 | 1 |
| 22-40 | 2 | 2 | 3 |
| 41-65 | 3 | 2 | 2 |
| 66+ | 3 | 3 | 1 |

The original brief says both `41-65` and `64+`, so ages 64 and 65 overlap. The table above follows the provisional choice in `plan.md`: 41-65, then 66+. If the recruiter clarifies the intended bands, change this rule and its tests. Record the assumption in the app README.

A straightforward approach is to choose the three point values for the age band, then add the ones whose answer qualifies. Keep the appointment decision as a separate simple rule: score **4 or more** means the appointment message. If you add a method for that decision, test scores 3 and 4.

Useful scoring tests:

- No qualifying answers -> 0 in every age band. Here that means Q1=no, Q2=no, Q3=yes.
- Q3=no alone -> its points for each band.
- Q1=yes and Q2=yes at age 18 -> 3.
- All three qualifying answers at age 18 -> 4.
- Q1=yes and Q2=yes at age 25 -> 4.
- Ages 16, 21, 22, 40, 41, 65, and 66 -> correct band.
- An age under 16 -> reject it or otherwise make the rule's behaviour explicit.

Try to avoid repeating the scoring chart in many places in the app. For Part Two, one clear implementation is enough; configuration belongs to optional Part Three.

## 5. Write the tests as you go

In xUnit, `[Fact]` is for one case and `[Theory]` with `[InlineData(...)]` is useful for several inputs to the same rule. Give tests names that describe behaviour, such as `GetAge_BeforeBirthday_SubtractsOne`. Use fixed dates, not today's real date, so the suite behaves the same next year.

A good loop is:

1. Write one test for a rule.
2. Run `dotnet test LifestyleChecker.slnx` from `JHallTest`.
3. Add just enough code to pass it.
4. Add boundary and failure cases.
5. Run the whole suite again.

If a test fails, read the expected and actual values first, then check whether the test or the rule misunderstood the brief. `dotnet build LifestyleChecker.slnx` is useful for separating compiler errors from test failures.

## When step 2 is done

- The age, matching, and scoring rules compile and have focused passing tests.
- Tests cover birthdays, mismatches, scoring boundaries, and the 3/4 threshold.
- None of these rules calls the API, reads a secret, depends on Razor Pages, or uses the current clock internally.
- The age-band assumption is documented.

It is fine if the website still shows the default starter page at this point. Wiring these rules into the forms and API happens in the following steps.
