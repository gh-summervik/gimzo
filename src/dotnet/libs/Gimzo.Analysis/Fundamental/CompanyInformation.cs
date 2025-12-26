namespace Gimzo.Analysis.Fundamental;

public sealed class CompanyInformation
{
    public required string CentralIndexKey { get; init; }
    public required string Exchange { get; init; }
    public required string Symbol { get; init; }
    public required string? Registrant { get; init; }
    public required string? Isin { get; init; }
    public required string? Lei { get; init; }
    public required string? Ein { get; init; }
    public required string? SicCode { get; init; }
    public required string? SicDescription { get; init; }
    public required string? FiscalYearEnd { get; init; }
    public required string? StateOfIncorporation { get; init; }
    public required string? PhoneNumber { get; init; }
    public required string? MailingAddress { get; init; }
    public required string? BusinessAddress { get; init; }
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
