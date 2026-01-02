using Gimzo.AppServices.Backtesting;

namespace Gimzo.AppServices.Tests;

public class BacktestingTests
{
    [Fact]
    public void CalculateSlope_Linear()
    {
        double[] data1 = [1D, 2D, 3D, 4D, 5D];
        double[] data2 = [-1D, -2D, -3D, -4D, -5D];
        var slope1 = BacktestingService.CalculateSlope(data1);
        var slope2 = BacktestingService.CalculateSlope(data2);
        Assert.Equal(1D, slope1);
        Assert.Equal(-1D, slope2);
    }
}
