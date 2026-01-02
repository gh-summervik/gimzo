namespace Gimzo.AppServices.Tests;

public class MiscTests
{
    [Fact]
    public void RangeExpressionTests()
    {
        int[] baseRange = [1, 2, 3, 4, 5];

        var range = baseRange[0..];
        Assert.Equal(baseRange, range);

        range = baseRange[1..];
        Assert.Equal(new int[] { 2, 3, 4, 5 }, range);

        range = baseRange[0..1];
        Assert.Equal(new int[] { 1 }, range);

        for (int i = 0; i < baseRange.Length; i++)
        {
            range = baseRange[0..i];
            Assert.True(range.Length == i);
        }
    }
}
