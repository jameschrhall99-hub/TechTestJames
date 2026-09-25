using System.Net;
using System.Text;
using LifestyleChecker.Api;
using LifestyleChecker.Models;

namespace LifestyleChecker.Tests.Api;

public class PatientApiClientTests
{
    [Fact]
    public async Task PatientApiTest_AllReturns()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"nhsNumber\":\"123456789\",\"name\":\"DOE, John\",\"born\":\"25-12-1990\"}",
                    Encoding.UTF8,
                    "application/json")
            });

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.Success, result.Status);
        Assert.NotNull(result.Patient);
        Assert.Equal("123456789", result.Patient.NhsNumber);
        Assert.Equal("DOE, John", result.Patient.Name);
        Assert.Equal(new DateOnly(1990, 12, 25), result.Patient.Born);
    }


    [Fact]
    public async Task PatientApiTest_404ReturnsNotFound()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound){});

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.NotFound, result.Status);
        Assert.Null(result.Patient);
    }

    [Fact]
    public async Task PatientApiTest_UnavailableReturnsUnavailable()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable){});

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.Unavailable, result.Status);
        Assert.Null(result.Patient);
    }

    [Fact]
    public async Task PatientApiTest_MalformedJson()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"nhsNumber\":\"123456789\" \"name\":\"DOE, John\",\"born\":\"25-12-1990\"}",
                    Encoding.UTF8,
                    "application/json")
            });

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.Unavailable, result.Status);
        Assert.Null(result.Patient);
    }

    [Fact]
    public async Task PatientApiTest_MissingNHSNumber()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"nhsNumber\":\"\" \"name\":\"DOE, John\",\"born\":\"25-12-1990\"}",
                    Encoding.UTF8,
                    "application/json")
            });

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.Unavailable, result.Status);
        Assert.Null(result.Patient);
    }

    [Fact]
    public async Task PatientApiTest_InvalidDOB()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    //DOB month = 15
                    "{\"nhsNumber\":\"123456789\",\"name\":\"DOE, John\",\"born\":\"25-15-1990\"}",
                    Encoding.UTF8,
                    "application/json")
            });

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.Unavailable, result.Status);
        Assert.Null(result.Patient);
    }

    [Fact]
    public async Task PatientApiTest_Timeout()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
            {
                Content = new StringContent(
                    //DOB month = 15
                    "{\"nhsNumber\":\"123456789\",\"name\":\"DOE, John\",\"born\":\"25-12-1990\"}",
                    Encoding.UTF8,
                    "application/json")
            });

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.Unavailable, result.Status);
        Assert.Null(result.Patient);
    }

    [Fact]
    public async Task PatientApiTest_HttpRequestException()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            throw new HttpRequestException("network failure"));

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.Unavailable, result.Status);
        Assert.Null(result.Patient);
    }




    [Fact]
    public async Task PatientApiTest_SendsExpectedRequestDetails()
    {
        //setup test constants
        const string testKey = "test-key";

        using var handler = new FakeHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(
                "https://al-tech-test-apim.azure-api.net/tech-test/t2/patients/123456789",
                request.RequestUri?.AbsoluteUri);
            Assert.Equal(
                testKey,
                Assert.Single(request.Headers.GetValues("Ocp-Apim-Subscription-Key")));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"nhsNumber\":\"123456789\",\"name\":\"DOE, John\",\"born\":\"25-12-1990\"}",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        // Use a fake key, doesn't call network
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", testKey);

        var apiClient = new PatientApiClient(httpClient);
        var result = await apiClient.GetPatientAsync("123456789");

        Assert.Equal(PatientLookupStatus.Success, result.Status);
    }

    [Fact]
    public async Task PatientApiTest_TaskCanceledException()
    {
        //setup test constants
        using var handler = new FakeHttpMessageHandler(_ =>
            throw new TaskCanceledException("timeout"));

        using var httpClient = new HttpClient(handler)
        {
            //test builds full request uri, api key not used though
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        //call client with nhs number.
        var result = await apiClient.GetPatientAsync("123456789");

        //check both result status and parsed patient details
        Assert.Equal(PatientLookupStatus.Unavailable, result.Status);
        Assert.Null(result.Patient);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345x789")]
    public async Task PatientApiTest_InvalidNhsNumber(string nhsNumber)
    {
        var requestWasSent = false;

        using var handler = new FakeHttpMessageHandler(_ =>
        {
            requestWasSent = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://al-tech-test-apim.azure-api.net/")
        };

        var apiClient = new PatientApiClient(httpClient);

        var result = await apiClient.GetPatientAsync(nhsNumber);

        Assert.Equal(PatientLookupStatus.Unavailable, result.Status);
        Assert.False(requestWasSent);
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
