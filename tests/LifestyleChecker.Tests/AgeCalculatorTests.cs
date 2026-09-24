using MyAgeCalculator;
using Xunit;

namespace MyAgeCalculatorTests;

public class AgeCalculatorTests
{
    [Fact]
    public void CalculateAgeTestBasic()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(1983, 3, 16);
        var today = new DateOnly(2026, 9, 23);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(43, age);
    }

    [Fact]
    public void ThrowsWhenDateOfBirthIsTomorrow()
    {
        //setup future birth date
        var dateOfBirth = new DateOnly(2026, 9, 24);
        var today = new DateOnly(2026, 9, 23);

        //ensure test throws error
        Assert.Throws<ArgumentOutOfRangeException>(() => AgeCalculator.GetAge(dateOfBirth, today));

    }

    [Fact]
    public void ThrowsWhenDateOfBirthIsInFutureYear()
    {
        //setup future birth date
        var dateOfBirth = new DateOnly(2027, 9, 24);
        var today = new DateOnly(2026, 9, 23);

        //ensure test throws error
        Assert.Throws<ArgumentOutOfRangeException>(() => AgeCalculator.GetAge(dateOfBirth, today));

    }

    [Fact]
    public void CalculateAgeTestBirthday()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(2010, 9, 23);
        var today = new DateOnly(2026, 9, 23);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(16, age);
    }

    [Fact]
    public void CalculateAgeTestDayBefore()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(2000, 9, 22);
        var today = new DateOnly(2026, 9, 21);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(25, age);
    }

    [Fact]
    public void CalculateAgeTestDayAfter()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(2000, 9, 22);
        var today = new DateOnly(2026, 9, 23);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(26, age);
    }

    [Fact]
    public void LeapYearBirthdayTest_OnNonLeapYear()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(2000, 2, 29);
        var today = new DateOnly(2026, 2, 28);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(25, age);
        
    }

    [Fact]
    public void LeapYearBirthdayTest_OnNonLeapYearBirthdayToday()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(2000, 2, 29);
        var today = new DateOnly(2026, 3, 1);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(26, age);
        
    }

    [Fact]
    public void LeapYearBirthdayTest_OnLeapYearBirthdayTomorrow()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(2000, 2, 29);
        var today = new DateOnly(2020, 2, 28);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(19, age);
        
    }

    [Fact]
    public void LeapYearBirthdayTest_OnLeapYearBirthdayToday()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(2000, 2, 29);
        var today = new DateOnly(2020, 2, 29);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(20, age);
        
    }

    [Fact]
    public void LeapYearBirthdayTest_OnLeapYearBirthdayYesterday()
    {
        //setup test constants
        var dateOfBirth = new DateOnly(2000, 2, 29);
        var today = new DateOnly(2020, 3, 1);

        //calculate age
        var age = AgeCalculator.GetAge(dateOfBirth, today);

        //check method calculation is correct
        Assert.Equal(20, age);
        
    }
}