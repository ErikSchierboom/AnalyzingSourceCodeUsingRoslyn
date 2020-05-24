using Xunit;

public class AnalyzerExerciseTests
{
    [Fact]
    public void Greeting_WithoutName_UsesDefault()
    {
        Assert.Equal("Hello you!", AnalyzerExercise.Greeting());
    }

    [Fact]
    public void Greeting_WithName_UsesName()
    {
        Assert.Equal("Hello Jane!", AnalyzerExercise.Greeting("Jane"));
    }
}

