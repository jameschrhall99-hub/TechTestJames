using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyRiskScorer;

namespace LifestyleChecker.Pages;

public class PartTwoModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please provide an answer to question 1.")]
    public bool? Q1Yes { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please provide an answer to question 2.")]
    public bool? Q2Yes { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please provide an answer to question 3.")]
    public bool? Q3Yes { get; set; }

    public IActionResult OnGet()
    {
        int? verifiedAge = HttpContext.Session.GetInt32("VerifiedAge");

        if(HttpContext.Session.GetString("PartOnePassed") != "true" || verifiedAge == null || verifiedAge < 16)
        {
            return RedirectToPage("/Index");
        }
        else if(HttpContext.Session.GetString("PartOnePassed") == "true" && HttpContext.Session.GetInt32("VerifiedAge") >= 16)
        {
            return Page();
        }

        return RedirectToPage("/Index");    
    }

    public IActionResult OnPost()
    {
        int? verifiedAge = HttpContext.Session.GetInt32("VerifiedAge");

        if(HttpContext.Session.GetString("PartOnePassed") != "true" || verifiedAge == null || verifiedAge < 16)
        {
            return RedirectToPage("/Index");
        }

        if(Q1Yes == null || Q2Yes == null || Q3Yes == null)
        {
            return Page();
        }

        if(!ModelState.IsValid)
        {
            return Page();
        }
        
        bool verifiedq1Yes = Q1Yes.Value;
        bool verifiedq2Yes = Q2Yes.Value;
        bool verifiedq3Yes = Q3Yes.Value;

        int score = RiskScorer.CalculateRisk(verifiedAge.Value, verifiedq1Yes, verifiedq2Yes, verifiedq3Yes);

        if(score <= 3)
        {
            HttpContext.Session.SetString("ResultCategory", "Low");
        }
        else if(score >= 4)
        {
            HttpContext.Session.SetString("ResultCategory", "High");
        }

        HttpContext.Session.Remove("PartOnePassed");
        HttpContext.Session.Remove("VerifiedAge");

        return RedirectToPage("/Result");


    }


}