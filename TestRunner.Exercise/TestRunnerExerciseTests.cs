using Xunit;

public class TestRunnerExerciseTests
{
    [Fact]
    public void Year_not_divisible_by_4_in_common_year()
    {
        Assert.False(TestRunnerExercise.IsLeapYear(2015));
    }

    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Year_divisible_by_2_not_divisible_by_4_in_common_year()
    {
        Assert.False(TestRunnerExercise.IsLeapYear(1970));
    }

    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Year_divisible_by_4_not_divisible_by_100_in_leap_year()
    {
        Assert.True(TestRunnerExercise.IsLeapYear(1996));
    }

    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Year_divisible_by_4_and_5_is_still_a_leap_year()
    {
        Assert.True(TestRunnerExercise.IsLeapYear(1960));
    }

    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Year_divisible_by_100_not_divisible_by_400_in_common_year()
    {
        Assert.False(TestRunnerExercise.IsLeapYear(2100));
    }

    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Year_divisible_by_100_but_not_by_3_is_still_not_a_leap_year()
    {
        Assert.False(TestRunnerExercise.IsLeapYear(1900));
    }

    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Year_divisible_by_400_in_leap_year()
    {
        Assert.True(TestRunnerExercise.IsLeapYear(2000));
    }

    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Year_divisible_by_400_but_not_by_125_is_still_a_leap_year()
    {
        Assert.True(TestRunnerExercise.IsLeapYear(2400));
    }

    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Year_divisible_by_200_not_divisible_by_400_in_common_year()
    {
        Assert.False(TestRunnerExercise.IsLeapYear(1800));
    }
}