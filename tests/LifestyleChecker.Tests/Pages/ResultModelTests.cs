using LifestyleChecker.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifestyleChecker.Tests.Pages;

public class ResultModelTests
{
    [Fact]
    public void MissingSessionCatagory()
    {
        //setup session
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new ResultModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };
        
        var result = model.OnGet();

        //empty catagory redirects to Index page
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
    }

    [Fact]
    public void UnexpectedSessionCatagory()
    {
        //setup session
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new ResultModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        session.SetString("ResultCategory", "Unexpected");
        
        var result = model.OnGet();

        //unexpected catagory redirects to Index page
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
    }

    [Fact]
    public void LowSessionCatagory()
    {
        //setup session
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new ResultModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        session.SetString("ResultCategory", "Low");
        
        var result = model.OnGet();

        Assert.Equal("Thank you for answering our questions, we don't need to see you at this time. Keep up the good work!", model.OutcomeMessage);
    }

    [Fact]
    public void HighSessionCatagory()
    {
        //setup session
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new ResultModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        session.SetString("ResultCategory", "High");
        
        var result = model.OnGet();

        Assert.Equal("We think there are some simple things you could do to improve your quality of life, please phone to book an appointment", model.OutcomeMessage);
    }

    [Fact]
    public void HighSessionCatagory_BrowserRefresh()
    {
        //setup session
        var session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = session };
        var model = new ResultModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        session.SetString("ResultCategory", "High");
        
        var result = model.OnGet();

        Assert.Equal("We think there are some simple things you could do to improve your quality of life, please phone to book an appointment", model.OutcomeMessage);

        result = model.OnGet();

        Assert.Equal("High", session.GetString("ResultCategory"));
        Assert.Equal("We think there are some simple things you could do to improve your quality of life, please phone to book an appointment", model.OutcomeMessage);
        
    }
}