namespace Gimzo.Analysis.Fundamental;

public sealed class CompanyInformation
{
    public CompanyInformation() { }
    public required string CentralIndexKey { get; init; }
    public required string Exchange { get; init; }
    public required string Symbol { get; init; }
    public string? Registrant { get; init; }
    public string? Isin { get; init; }
    public string? Lei { get; init; }
    public string? Ein { get; init; }
    public string? SicCode { get; init; }
    public string? SicDescription { get; init; }
    public string? SicTitle { get; init;  }
    public string? FiscalYearEnd { get; init; }
    public string? StateOfIncorporation { get; init; }
    public string? PhoneNumber { get; init; }
    public string? MailingAddress { get; init; }
    public string? BusinessAddress { get; init; }
    public string? FormerName { get; init; }
    public string? Industry { get; init; }
    public string? DateFounding { get; init; }
    public string? ChiefExecutiveOfficer { get; init; }
    public int? NumberEmployees { get; init; }
    public string? WebSite { get; init; }
    public decimal? MarketCap { get; init; }
    public double? SharesIssued { get; init; }
    public double? SharesOutstanding { get; init; }
    public string? Description { get; init; }
}
