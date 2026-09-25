using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifestyleChecker.Pages;

public class ResultModel : PageModel
{
    public string OutcomeMessage { get; private set; } = string.Empty;

    public IActionResult OnGet()
    {
        string category = HttpContext.Session.GetString("ResultCategory");

        if(category == "High")
        {
            OutcomeMessage = "We think there are some simple things you could do to improve you quality of life, please phone to book an appointment";           
            return Page();
        }
        else if(category == "Low")
        {
            OutcomeMessage = "Thank you for answering our questions, we don't need to see you at this time. Keep up the good work!";
            return Page();
        }

        return RedirectToPage("/Index"); 
    }

}