using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.AppServices.Models;

public record CompanyInfo
{
    private string? _fiscalYearEnd;

    public string? Symbol { get; set; }
    public string? CentralIndexKey { get; set; }
    public string? Exchange { get; set; }
    public string? Registrant { get; set; }
    public string? Industry { get; set; }
    public string? Isin { get; set; }
    public string? Lei { get; set; }
    public string? Ein { get; set; }
    public string? Sic { get; set; }
    public string? FiscalYearEnd
    {
        get => _fiscalYearEnd;
        set => _fiscalYearEnd = (value?.Length ?? 0) == 4
                ? string.Concat(value![0..2], "/", value[2..])
                : value;
    }
    public string? StateOfIncorporation { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MailingAddress { get; set; }
    public string? BusinessAddress { get; set; }
    public string? FormerName { get; set; }
    public string? DateFounding { get; set; }
    public string? ChiefExecutiveOfficer { get; set; }
    public int? NumberEmployees { get; set; }
    public string? WebSite { get; set; }
    public decimal? MarketCap { get; set; }
    public double? SharesIssued { get; set; }
    public double? SharesOutstanding { get; set; }
    public string? Description { get; set; }
    public Ohlc? LastOhlc { get; set; }
    public AverageTrueRange? CurrentAverageTrueRange { get; set; }
    public decimal? FiftyTwoWeekLow { get; set;  }
    public decimal? FiftyTwoWeekHigh { get; set; }
    public long? TwentyDayAverageVolume { get; set; }
    public double? LastTrendValue { get; set;  }
}
