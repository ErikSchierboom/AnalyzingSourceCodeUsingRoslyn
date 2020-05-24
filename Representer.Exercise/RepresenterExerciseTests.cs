using Xunit;

public class RepresenterExerciseTests
{
    [Fact]
    public void Year_not_divisible_by_4_in_common_year()
    {
        Assert.False(RepresenterExercise.IsLeapYear(2015));
    }

    [Fact]
    public void Year_divisible_by_2_not_divisible_by_4_in_common_year()
    {
        Assert.False(RepresenterExercise.IsLeapYear(1970));
    }

    [Fact]
    public void Year_divisible_by_4_not_divisible_by_100_in_leap_year()
    {
        Assert.True(RepresenterExercise.IsLeapYear(1996));
    }

    [Fact]
    public void Year_divisible_by_4_and_5_is_still_a_leap_year()
    {
        Assert.True(RepresenterExercise.IsLeapYear(1960));
    }

    [Fact]
    public void Year_divisible_by_100_not_divisible_by_400_in_common_year()
    {
        Assert.False(RepresenterExercise.IsLeapYear(2100));
    }

    [Fact]
    public void Year_divisible_by_100_but_not_by_3_is_still_not_a_leap_year()
    {
        Assert.False(RepresenterExercise.IsLeapYear(1900));
    }

    [Fact]
    public void Year_divisible_by_400_in_leap_year()
    {
        Assert.True(RepresenterExercise.IsLeapYear(2000));
    }

    [Fact]
    public void Year_divisible_by_400_but_not_by_125_is_still_a_leap_year()
    {
        Assert.True(RepresenterExercise.IsLeapYear(2400));
    }

    [Fact]
    public void Year_divisible_by_200_not_divisible_by_400_in_common_year()
    {
        Assert.False(RepresenterExercise.IsLeapYear(1800));
    }
}