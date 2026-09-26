using LifestyleChecker.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifestyleChecker.Tests.Pages;

public class PartTwoModelTests
{
    [Fact]
    public void OnGet_WithoutSessionValues_RedirectsToIndex()
    {
        //create a page model with empty session
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        //try to open part two without passing part one check
        var result = model.OnGet();

        //user sent back to the start page
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
    }

    [Fact]
    public void OnGet_WithoutAge_RedirectsToIndex()
    {
        //create a page model
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        //set passed to true, leave age empty
        session.SetString("PartOnePassed", "true");

        //try to open part two without passing part one check
        var result = model.OnGet();

        //user sent back to the start page
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
    }

    [Fact]
    public void OnGet_WithoutPassFlag_RedirectsToIndex()
    {
        //create a page model
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        //set age to 20, leave passed empty
        session.SetInt32("VerifiedAge", 20);

        //try to open part two without passing part one check
        var result = model.OnGet();

        //user sent back to the start page
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
    }

    [Fact]
    public void OnGet_TooYoung_RedirectsToIndex()
    {
        //create a page model
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        //set age to 15, PassFlag to true
        session.SetInt32("VerifiedAge", 15);
        session.SetString("PartOnePassed", "true");

        //try to open part two without passing part one check
        var result = model.OnGet();

        //user sent back to the start page
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
    }

    [Fact]
    public void OnGet_AllCorrect_ReturnsPageResult()
    {
        //create a page model
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        //set age to 20, PassFlag to true
        session.SetInt32("VerifiedAge", 20);
        session.SetString("PartOnePassed", "true");

        //try to open part two
        var result = model.OnGet();

        //user sent to correct page
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnPost_EmptySession_RedirectsToIndex()
    {
        //provide answers but no part one session values
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = true
        };

        //submit the answers without passing part one check
        var result = model.OnPost();

        //user sent back to the start page and session is empty
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public void OnPost_IncompleteSessionAge_RedirectsToIndex()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = true
        };

        //set passed to true, leave age empty
        session.SetString("PartOnePassed", "true");

        //submit the answers without passing part one check
        var result = model.OnPost();

        //user sent back to the start page and session is empty
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public void OnPost_IncompleteSessionPass_RedirectsToIndex()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = true
        };

        //set age to 20, leave passed empty
        session.SetInt32("VerifiedAge", 20);

        //submit the answers without passing part one check
        var result = model.OnPost();

        //user sent back to the start page and session is empty
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public void OnPost_TooYoung_RedirectsToIndex()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = true
        };

        //set age to 15, pass to true
        session.SetInt32("VerifiedAge", 15);
        session.SetString("PartOnePassed", "true");

        //submit the answers without passing part one check
        var result = model.OnPost();

        //user sent back to the start page and session is empty
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public void RequiredAnswers_Missing1_ReturnsPage()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = null,
            Q2Yes = true,
            Q3Yes = true
        };

        //set age to 20, pass to true
        session.SetInt32("VerifiedAge", 20);
        session.SetString("PartOnePassed", "true");

        //q1 missing error
        model.ModelState.AddModelError(nameof(model.Q1Yes), "Please provide an answer to question 1.");

        //submit the answers
        var result = model.OnPost();

        //user sent back to answer question 1 and session is empty
        Assert.IsType<PageResult>(result);
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public void RequiredAnswers_Missing2_ReturnsPage()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = null,
            Q3Yes = true
        };

        //set age to 20, pass to true
        session.SetInt32("VerifiedAge", 20);
        session.SetString("PartOnePassed", "true");

        //q2 missing error
        model.ModelState.AddModelError(nameof(model.Q2Yes), "Please provide an answer to question 2.");

        //submit the answers
        var result = model.OnPost();

        //user sent back to answer question 2 and session is empty
        Assert.IsType<PageResult>(result);
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public void RequiredAnswers_Missing3_ReturnsPage()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = null
        };

        //set age to 20, pass to true
        session.SetInt32("VerifiedAge", 20);
        session.SetString("PartOnePassed", "true");

        //q3 missing error
        model.ModelState.AddModelError(nameof(model.Q3Yes), "Please provide an answer to question 3.");

        //submit the answers
        var result = model.OnPost();

        //user sent back to answer question 3 and session is empty
        Assert.IsType<PageResult>(result);
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public void CheckCorrectSCore_Low()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = true
        };

        //set age to 20, pass to true
        session.SetInt32("VerifiedAge", 20);
        session.SetString("PartOnePassed", "true");



        //submit the answers
        var result = model.OnPost();

        //result is expected, and session empties
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Result", redirect.PageName);
        Assert.Equal("Low", session.GetString("ResultCategory"));
        Assert.Null(session.GetString("PartOnePassed"));
        Assert.Null(session.GetInt32("VerifiedAge"));
    }

    [Fact]
    public void CheckCorrectSCore_High()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = false
        };

        //set age to 20, pass to true
        session.SetInt32("VerifiedAge", 20);
        session.SetString("PartOnePassed", "true");



        //submit the answers
        var result = model.OnPost();

        //result is expected, and session empties
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Result", redirect.PageName);
        Assert.Equal("High", session.GetString("ResultCategory"));
        Assert.Null(session.GetString("PartOnePassed"));
        Assert.Null(session.GetInt32("VerifiedAge"));
    }

    [Fact]
    public void CheckSecondPostRedirects()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = false
        };

        //set age to 20, pass to true
        session.SetInt32("VerifiedAge", 20);
        session.SetString("PartOnePassed", "true");

        //submit the answers
        var result = model.OnPost();

        //call OnPost again
        result = model.OnPost();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
        Assert.Equal("High", session.GetString("ResultCategory"));
    }

    [Fact]
    public void CheckInvalidModelState()
    {
        //provide answers
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new PartTwoModel
        {
            PageContext = new PageContext { HttpContext = httpContext },
            Q1Yes = true,
            Q2Yes = true,
            Q3Yes = true
        };

        //set age to 20, pass to true
        session.SetInt32("VerifiedAge", 20);
        session.SetString("PartOnePassed", "true");

        //add error
        model.ModelState.AddModelError(nameof(model.Q2Yes), "Invalid answer.");

        //submit the answers
        var result = model.OnPost();

        //result is expected
        Assert.IsType<PageResult>(result);
        Assert.Null(session.GetString("ResultCategory"));
        Assert.Equal("true", session.GetString("PartOnePassed"));
        Assert.Equal(20, session.GetInt32("VerifiedAge"));
    }
}
