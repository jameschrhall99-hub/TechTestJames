using MyRiskScorer;
using Xunit;

namespace MyRiskScorerTests;

public class RiskScorerTests
{
    [Fact]
    public void Q1AndQ2AnswerYesTest_Young()
    {
        //setup test constants
        int age = 18;
        bool q1Yes = true;
        bool q2Yes = true;
        bool q3Yes = true;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(3, result);
    }

    [Fact]
    public void AllAnswersAddPointsTest_Young()
    {
        //setup test constants
        int age = 18;
        bool q1Yes = true;
        bool q2Yes = true;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(4, result);
    }

    [Fact]
    public void Q1AndQ2AnswerYesTest_Adult()
    {
        //setup test constants
        int age = 25;
        bool q1Yes = true;
        bool q2Yes = true;
        bool q3Yes = true;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(4, result);
    }

    [Fact]
    public void Q3AnswerNoTest_Young()
    {
        //setup test constants
        int age = 18;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(1, result);
    }

    [Fact]
    public void Q3AnswerNoTest_Adult()
    {
        //setup test constants
        int age = 25;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(3, result);
    }

    [Fact]
    public void Q3AnswerNoTest_Middle()
    {
        //setup test constants
        int age = 45;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(2, result);
    }

    [Fact]
    public void Q3AnswerNoTest_Senior()
    {
        //setup test constants
        int age = 70;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(1, result);
    }

    [Fact]
    public void Q3AnswerYesTest_Young()
    {
        //setup test constants
        int age = 18;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = true;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(0, result);
    }

    [Fact]
    public void Q3AnswerYesTest_Adult()
    {
        //setup test constants
        int age = 25;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = true;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(0, result);
    }

    [Fact]
    public void Q3AnswerYesTest_Middle()
    {
        //setup test constants
        int age = 45;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = true;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(0, result);
    }

    [Fact]
    public void Q3AnswerYesTest_Senior()
    {
        //setup test constants
        int age = 70;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = true;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(0, result);
    }

    [Fact]
    public void CheckBoundaryAges_YoungLow()
    {
        //setup test constants
        int age = 16;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(1, result);
    }

    [Fact]
    public void CheckBoundaryAges_YoungHigh()
    {
        //setup test constants
        int age = 21;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(1, result);
    }

    [Fact]
    public void CheckBoundaryAges_AdultLow()
    {
        //setup test constants
        int age = 22;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(3, result);
    }

    [Fact]
    public void CheckBoundaryAges_AdultHigh()
    {
        //setup test constants
        int age = 40;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(3, result);
    }

    [Fact]
    public void CheckBoundaryAges_MiddleLow()
    {
        //setup test constants
        int age = 41;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(2, result);
    }

    [Fact]
    public void CheckBoundaryAges_MiddleHigh()
    {
        //setup test constants
        int age = 65;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(2, result);
    }

    [Fact]
    public void CheckBoundaryAges_Senior()
    {
        //setup test constants
        int age = 66;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //calculate risk score
        int result = RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes);

        //check method calculation is correct
        Assert.Equal(1, result);
    }

    [Fact]
    public void CheckInvalidAgeThrows_Under16()
    {
     //setup test constants
        int age = 15;
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //check method thows error
        Assert.Throws<ArgumentOutOfRangeException>(() => RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes));
    }

    [Fact]
    public void CheckInvalidAgeThrows_NoAge()
    {
     //setup test constants
        int age = new int();
        bool q1Yes = false;
        bool q2Yes = false;
        bool q3Yes = false;

        //check method thows error
        Assert.Throws<ArgumentOutOfRangeException>(() => RiskScorer.CalculateRisk(age, q1Yes, q2Yes, q3Yes));
    }
}
