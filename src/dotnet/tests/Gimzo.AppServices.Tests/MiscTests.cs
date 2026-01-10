using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.AppServices.Backtests;
using Gimzo.Common;
using System.Text.Json;
using Xunit.Abstractions;

namespace Gimzo.AppServices.Tests;

public class MiscTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public void RangeExpressionTests()
    {
        int[] baseRange = [1, 2, 3, 4, 5];

        var range = baseRange[0..];
        Assert.Equal(baseRange, range);

        range = baseRange[1..];
        Assert.Equal([2, 3, 4, 5], range);

        range = baseRange[0..1];
        Assert.Equal([1], range);

        for (int i = 0; i < baseRange.Length; i++)
        {
            range = baseRange[0..i];
            Assert.True(range.Length == i);
        }
    }

    /*
     * Not really a test - used to build backtest config JSON.
     */
    [Fact]
    public void BuildBacktestConfigForPxfl()
    {
        var config = new BacktestConfiguration()
        {
            Chart = new ChartConfiguration()
            {
                RelativeStrengthPeriod = 14,
                AverageTrueRangePeriod = 14,
                MovingAverages = ["S21C", "S50C", "S200C"],
                BollingerBand = new BollingerBandConfiguration()
                {
                    MovingAverageCode = "S21C",
                    StdDevMultipler = 2D
                },
                Interval = ChartInterval.Daily
            },
            Name = BacktestingService.Keys.PriceLongExtremeFollow,
            SymbolCriteria = new SymbolConfiguration()
            {
                MinAbsoluteScore = 40,
                MinCompanyPercentileRank = 50,
                MaxIndustryRank = 150
            },
            PriceExtremeFollow = new PriceExtremeFollowConfiguration()
            {
                LookbackPeriod = 14,
                MovingAveragePeriod = 21,
                EntryCriteria = new()
                {
                    MinPercentFromPrevHigh = -20D,
                    MaxPercentFromPrevHigh = -5D,
                    MinRsi = -5D,
                    MaxRsi = 5D,
                    MinRelativeVolume = 1.6D,
                    MinPriorSlope = 0.1D,
                    MinPercentFromMovingAverage = 4,
                    MinAverageTrueRange = 2M
                }
            }
        };

        var options = JsonOptionsRepository.Gimzo;
        options.Converters.Add(new EnumDescriptionConverter<PricePoint>());
        options.Converters.Add(new EnumDescriptionConverter<MovingAverageType>());
        options.Converters.Add(new EnumDescriptionConverter<ChartInterval>());
        var json = JsonSerializer.Serialize(config, options);

        _output.WriteLine(json);
    }
}
