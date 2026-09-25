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
    public async Task OnPostAsync_ValidAdultWithMatchingDetails_RedirectsToQuestionnaire()
    {
        //use DOB 30 years ago so the patient is an adult always
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
