/*

assuming data will always come from api like example given

{
  "nhsNumber": "123456789",
  "name": "DOE, John",
  "born": "25-12-1990"
}

*/

namespace MyPatientMatcher;

public class PatientMatcher
{
    public bool IsMatch(string enteredNHSNumber, string enteredSurname, DateOnly enteredDOB, string apiNHSNumber, string apiName, DateOnly apiDOB)
    {

        //check nhs numbers are populated
        if(string.IsNullOrWhiteSpace(enteredNHSNumber) || string.IsNullOrWhiteSpace(apiNHSNumber))
        {
            return false;
        }

        //immediately return false if no matching NHS number
        if(enteredNHSNumber != apiNHSNumber)
        {
            return false;
        }

        //check user has entered a surname
        if(string.IsNullOrWhiteSpace(enteredSurname))
        {
            return false;
        }

        //ensure case insensitive and remove trailing or leading whitespace
        string compareEnteredSurname = enteredSurname.Trim();

        //check api has a name
        if(string.IsNullOrWhiteSpace(apiName))
        {
            return false;
        }

        //check comma in api name
        if(!apiName.Contains(','))
        {
            return false;
        }

        //split name into surname and first name, assuming no commas in name
        string[] nameParts = apiName.Split(',');

        //apiSurname is surname given from api
        string compareApiSurname = nameParts[0].Trim();


        //check api name has a surname
        if(string.IsNullOrWhiteSpace(compareApiSurname))
        {
            return false;
        }


        //if names aren't equal return false
        if(!string.Equals(compareApiSurname, compareEnteredSurname, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if(enteredDOB == apiDOB)
        {
            return true;
        }

        return false;


    }
}