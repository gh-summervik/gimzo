namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record UsCompany : DaoBase
{
    public UsCompany() : base()
    {
        CentralIndexKey = "";
        Exchange = "";
        Symbol = "";
    }

    public UsCompany(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        Exchange = "";
        Symbol = "";
    }

    public string CentralIndexKey { get; init; }
    public string Exchange { get; init; }
    public string Symbol { get; init; }
    public string? Registrant { get; init; }
    public string? Isin { get; init; }
    public string? Lei { get; init; }
    public string? Ein { get; init; }
    public string? SicCode { get; init; }
    public string? SicDescription { get; init; }
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
    public double? MarketCap { get; init; }
    public double? SharesIssued { get; init; }
    public double? SharesOutstanding { get; init; }
    public string? Description { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(Exchange) &&
        !string.IsNullOrWhiteSpace(Symbol);
}