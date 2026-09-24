using System.Globalization;
using System.Net;
using System.Text.Json;
using LifestyleChecker.Models;

namespace LifestyleChecker.Api;

//possible outcomes of looking up a patient.
public enum PatientLookupStatus
{
    Success,

    //HTTP 404 no patient was found for this number
    NotFound,

    //could not provide a usable response
    Unavailable
}

public record PatientLookupResult(

    //lookup succeeded, found no patient, or failed
    PatientLookupStatus Status,

    // contains patient details for success, null for other statuses
    PatientInfo? Patient = null);

public class PatientApiClient(HttpClient httpClient)
{
    // looks up a patient asynchronously using NHS number
    // cancellation token lets the caller cancel the HTTP request if needed
    public async Task<PatientLookupResult> GetPatientAsync(string nhsNumber, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(nhsNumber) || nhsNumber.Any(character => character < '0' || character > '9'))
        {
            return new PatientLookupResult(PatientLookupStatus.Unavailable);
        }

        try
        {
            using var response = await httpClient.GetAsync($"tech-test/t2/patients/{nhsNumber}", cancellationToken);

            // 404 means the request worked but patient does not exist
            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                return new PatientLookupResult(PatientLookupStatus.NotFound);
            }     

            // Anything other than 2xx is unavailable.
            if(!response.IsSuccessStatusCode)
            {
                return new PatientLookupResult(PatientLookupStatus.Unavailable);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if(root.ValueKind != JsonValueKind.Object)
            {
                return new PatientLookupResult(PatientLookupStatus.Unavailable);
            }

            //read files from json
            if(!root.TryGetProperty("nhsNumber", out var nhsNumberProperty) ||
                !root.TryGetProperty("name", out var nameProperty) ||
                !root.TryGetProperty("born", out var bornProperty))
            {
                return new PatientLookupResult(PatientLookupStatus.Unavailable);
            }


            if(nhsNumberProperty.ValueKind != JsonValueKind.String ||
                nameProperty.ValueKind != JsonValueKind.String ||
                bornProperty.ValueKind != JsonValueKind.String)
            {
                return new PatientLookupResult(PatientLookupStatus.Unavailable);
            }

            var returnedNhsNumber = nhsNumberProperty.GetString();
            var name = nameProperty.GetString();
            var born = bornProperty.GetString();

            //non populated fields are invalid
            if(string.IsNullOrWhiteSpace(returnedNhsNumber) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(born))
            {
                return new PatientLookupResult(PatientLookupStatus.Unavailable);
            }

            //parse api's birth date format
            if(!DateOnly.TryParseExact(
                born,
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                return new PatientLookupResult(PatientLookupStatus.Unavailable);
            }

            var patient = new PatientInfo(
                returnedNhsNumber,
                name,
                date);

            return new PatientLookupResult(PatientLookupStatus.Success, patient);
            
        }

        catch(OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            //http timeout
            return new PatientLookupResult(PatientLookupStatus.Unavailable);
        }
        catch(HttpRequestException)
        {
            //network failed
            return new PatientLookupResult(PatientLookupStatus.Unavailable);
        }
        catch(JsonException)
        {
            //invalid json
            return new PatientLookupResult(PatientLookupStatus.Unavailable);
        }
    }
}
