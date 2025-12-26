namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record InternationalCompany : DaoBase
{
    public InternationalCompany() : base()
    {
        Symbol = "";
        Exchange = "";
    }

    public InternationalCompany(Guid userId) : base(userId)
    {
        Symbol = "";
        Exchange = "";
    }

    public InternationalCompany(Guid userId, DataProviders.FinancialDataNet.InternationalCompanyInformation info) : base(userId)
    {
        Symbol = info.Symbol;
        Registrant = info.Registrant;
        Exchange = info.Exchange;
        Isin = info.Isin;
        Industry = info.Industry;
        YearFounding = info.YearFounding;
        ChiefExecutiveOfficer = info.ChiefExecutiveOfficer;
        NumberEmployees = info.NumberEmployees;
        WebSite = info.WebSite;
        Description = info.Description;
    }

    public string Symbol { get; init; }
    public string Exchange { get; init; }
    public string? Registrant { get; init; }
    public string? Isin { get; init; }
    public string? Industry { get; init; }
    public string? YearFounding { get; init; }
    public string? ChiefExecutiveOfficer { get; init; }
    public int? NumberEmployees { get; init; }
    public string? WebSite { get; init; }
    public string? Description { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        !string.IsNullOrWhiteSpace(Exchange);
}
