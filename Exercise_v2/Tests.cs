namespace Exercise_v2
{
    using Xunit;

    public class Tests
    {
        [Fact]
        public void Greeting_WithoutName_UsesDefault()
        {
            Assert.Equal("Hello there!", Implementation.Greeting());
        }

        [Fact(Skip = "Remove the Skip property to enable this test")]
        public void Greeting_WithName_UsesName()
        {
            Assert.Equal("Hello Jane!", Implementation.Greeting("Jane"));
        }
    }
}