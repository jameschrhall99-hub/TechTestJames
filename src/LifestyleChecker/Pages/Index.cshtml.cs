using System.ComponentModel.DataAnnotations;
using System.Globalization;
using LifestyleChecker.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyAgeCalculator;
using MyPatientMatcher;

namespace LifestyleChecker.Pages;

public class IndexModel(PatientApiClient patientApiClient) : PageModel
{
    private const string NotFoundMessage = "Your details could not be found";
    private const string UnderAgeMessage = "You are not eligble for this service";

    [BindProperty]
    [Required(ErrorMessage = "Enter your NHS number.")]
    [RegularExpression("^[0-9]+$", ErrorMessage = "Enter an NHS number using digits only.")]
    public string NhsNumber { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Enter your surname.")]
    public string Surname { get; set; } = string.Empty;

    //keep date as text so its yyyy-MM-dd form value can be parsed explicitly
    [BindProperty]
    [Required(ErrorMessage = "Enter your date of birth.")]
    public string DateOfBirth { get; set; } = string.Empty;

    public string? OutcomeMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (!DateOnly.TryParseExact(
                DateOfBirth,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var enteredDateOfBirth))
        {
            ModelState.AddModelError(nameof(DateOfBirth), "Enter a valid date of birth.");
        }
        else if (enteredDateOfBirth > today)
        {
            ModelState.AddModelError(nameof(DateOfBirth), "Date of birth cannot be in the future.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var lookup = await patientApiClient.GetPatientAsync(NhsNumber, cancellationToken);

        if (lookup.Status == PatientLookupStatus.NotFound)
        {
            OutcomeMessage = NotFoundMessage;
            return Page();
        }

        if (lookup.Status == PatientLookupStatus.Unavailable || lookup.Patient is null)
        {
            OutcomeMessage = "The patient service is currently unavailable. Please try again later.";
            return Page();
        }

        var patient = lookup.Patient;
        var matcher = new PatientMatcher();
        var detailsMatch = matcher.IsMatch(
            NhsNumber,
            Surname,
            enteredDateOfBirth,
            patient.NhsNumber,
            patient.Name,
            patient.Born);

        if (!detailsMatch)
        {
            OutcomeMessage = NotFoundMessage;
            return Page();
        }

        var age = AgeCalculator.GetAge(patient.Born, today);

        if (age < 16)
        {
            OutcomeMessage = UnderAgeMessage;
            return Page();
        }

        //store only the part one decision and verified age, not patient details
        HttpContext.Session.SetString("PartOnePassed", "true");
        HttpContext.Session.SetInt32("VerifiedAge", age);

        // The PartTwo page will be added in Step 4.
        return RedirectToPage("/PartTwo");
    }
}
