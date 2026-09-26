using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using LifestyleChecker.Api;
using LifestyleChecker.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifestyleChecker.Tests.Pages;

public class IndexModelTests
{
    [Fact]
    public void OnGet_ClearsExistingFlowSessionValues()
    {
        //create page model with flow values in session
        using var handler = new FakeHttpMessageHandler(_ =>
            throw new InvalidOperationException(" API should not be called in this test."));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };
        var session = new TestSession();
        session.SetString("PartOnePassed", "true");
        session.SetInt32("VerifiedAge", 18);
        session.SetString("ResultCategory", "Low");

        var pageModel = new IndexModel(new PatientApiClient(httpClient))
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { Session = session }
            }
        };

        //return to start page
        pageModel.OnGet();

        //previous flow cannot be resumed
        Assert.Null(session.GetString("PartOnePassed"));
        Assert.Null(session.GetInt32("VerifiedAge"));
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public async Task OnPostAsync_InvalidInput_ClearsExistingFlowSessionValues()
    {
        //create model with stale values and force validation to fail
        using var handler = new FakeHttpMessageHandler(_ =>
            throw new InvalidOperationException(" API should not be called in this test."));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };
        var session = new TestSession();
        session.SetString("PartOnePassed", "true");
        session.SetInt32("VerifiedAge", 18);
        session.SetString("ResultCategory", "Low");

        var pageModel = new IndexModel(new PatientApiClient(httpClient))
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { Session = session }
            }
        };
        pageModel.ModelState.AddModelError("TestInput", "Invalid test input.");

        //submit invalid input, api should not be called
        var result = await pageModel.OnPostAsync(CancellationToken.None);

        //page is shown again and stale flow values are cleared
        Assert.IsType<PageResult>(result);
        Assert.Null(session.GetString("PartOnePassed"));
        Assert.Null(session.GetInt32("VerifiedAge"));
        Assert.Null(session.GetString("ResultCategory"));
    }

    [Fact]
    public async Task OnPostAsync_ValidAdultWithMatchingDetails_RedirectsToQuestionnaire()
    {
        //use DOB 30 years ago so  patient is always adult
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dateOfBirth = today.AddYears(-30);
        var apiDateOfBirth = dateOfBirth.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

        var responseJson = JsonSerializer.Serialize(new
        {
            nhsNumber = "123456789",
            name = "DOE, John",
            born = apiDateOfBirth
        });

        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);
        var pageModel = new IndexModel(apiClient)
        {
            NhsNumber = "123456789",
            Surname = "Doe",
            DateOfBirth = dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        pageModel.HttpContext.Session = new TestSession();

        var result = await pageModel.OnPostAsync(CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/PartTwo", redirect.PageName);
        Assert.Equal("true", pageModel.HttpContext.Session.GetString("PartOnePassed"));
        Assert.Equal(30, pageModel.HttpContext.Session.GetInt32("VerifiedAge"));
    }
}

internal sealed class FakeHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(respond(request));
    }
}

internal sealed class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> values = new();

    public bool IsAvailable => true;
    public string Id => "test-session";
    public IEnumerable<string> Keys => values.Keys;

    public void Clear() => values.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public void Remove(string key) => values.Remove(key);

    public void Set(string key, byte[] value) => values[key] = value;

    public bool TryGetValue(string key, out byte[]? value) =>
        values.TryGetValue(key, out value);
}
