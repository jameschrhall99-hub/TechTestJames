using MyPatientMatcher;
using Xunit;

namespace MyPatientMatcherTests;

public class MyPatientMatcherTests
{
    [Fact]
    public void CheckAllMatchesTest()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456789";
        string enteredSurname = "Doe";
        string apiName = "DOE, John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.True(result);
    }

    [Fact]
    public void CheckUnmatchedNHSNumberFails()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456780";
        string enteredSurname = "Doe";
        string apiName = "DOE, John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }

    [Fact]
    public void CheckUnmatchedSurnameFails()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456789";
        string enteredSurname = "Do";
        string apiName = "DOE, John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }

    [Fact]
    public void CheckUnmatchedDOBFails()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456789";
        string enteredSurname = "Doe";
        string apiName = "DOE, John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 24);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }

    [Fact]
    public void CheckMalformedAPINameFails_NoSurname()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456789";
        string enteredSurname = "Doe";
        string apiName = " , John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }

    [Fact]
    public void CheckMalformedAPINameFails_NoComma()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456789";
        string enteredSurname = "Doe";
        string apiName = "DOE John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);
    }

    [Fact]
    public void CheckAllMatchesTest_WithSurnameWhitespace()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456789";
        string enteredSurname = "   doe   ";
        string apiName = " DOE  , John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.True(result);
    }

    [Fact]
    public void CheckEmptyEnteredNHSNUmberFails()
    {
        //setup test constants
        string enteredNHSNumber = "";
        string apiNHSNumber = "123456789";
        string enteredSurname = "Doe";
        string apiName = "DOE, John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }

    [Fact]
    public void CheckEmptyAPINHSNumberFails()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "";
        string enteredSurname = "Doe";
        string apiName = "DOE, John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }

    [Fact]
    public void CheckEmptyEnteredSurnameFails()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456789";
        string enteredSurname = "";
        string apiName = "DOE, John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }

    [Fact]
    public void CheckEmptyAPINameFails()
    {
        //setup test constants
        string enteredNHSNumber = "123456789";
        string apiNHSNumber = "123456789";
        string enteredSurname = "Doe";
        string apiName = "";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }

    [Fact]
    public void CheckWhiteSpaceAroundNHSNUmberFails()
    {
        //setup test constants
        string enteredNHSNumber = "  123456789  ";
        string apiNHSNumber = "123456789";
        string enteredSurname = "Doe";
        string apiName = "DOE, John";
        DateOnly enteredDOB = new DateOnly(1990, 12, 25);
        DateOnly apiDOB = new DateOnly(1990, 12, 25);

        //Create instance
        var matcher = new PatientMatcher();

        //run method with set constants
        bool result = matcher.IsMatch(enteredNHSNumber, enteredSurname, enteredDOB, apiNHSNumber, apiName, apiDOB);

        //check method returns correct result
        Assert.False(result);

    }
}