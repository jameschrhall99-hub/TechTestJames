namespace MyAgeCalculator;

public static class AgeCalculator
{
    public static int GetAge(DateOnly dateOfBirth, DateOnly today)
    {

        //error if birth date is in the future
        if(today < dateOfBirth)
        {
            throw new ArgumentOutOfRangeException(nameof(dateOfBirth));
        }

        //initial age assumption
        int age = today.Year - dateOfBirth.Year;

        DateOnly birthdayThisYear;

        //if born on Feb 29th and this year is leap year, they turn new age on 29th Feb, non leap years turn new age on 1st March
        if(dateOfBirth.Month == 2 && dateOfBirth.Day == 29)
        {  
            birthdayThisYear = DateTime.IsLeapYear(today.Year)
                ? new DateOnly(today.Year, 2, 29)
                : new DateOnly(today.Year, 3, 1);            
        }
        else
        {
            birthdayThisYear = new DateOnly(today.Year, dateOfBirth.Month, dateOfBirth.Day);
        }


        //if birthday hasn't happened yet age doesn't go up this year yet
        if(today < birthdayThisYear)
        {
            age--;
        }

        return age;

    }
}