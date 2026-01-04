namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record CompanyInformation : DaoBase
{
    public CompanyInformation() : base()
    {
        CentralIndexKey = "";
        Exchange = "";
        Symbol = "";
    }

    public CompanyInformation(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        Exchange = "";
        Symbol = "";
    }

    public CompanyInformation(Analysis.Fundamental.CompanyInformation companyInformation, Guid userId) : base(userId)
    {
        CentralIndexKey = companyInformation.CentralIndexKey;
        Exchange = companyInformation.Exchange;
        Symbol = companyInformation.Symbol;
        Registrant = companyInformation.Registrant;
        Isin = companyInformation.Isin;
        Lei = companyInformation.Lei;
        Ein = companyInformation.Ein;
        SicCode = companyInformation.SicCode;
        SicDescription = companyInformation.SicDescription;
        SicTitle = companyInformation.SicTitle;
        FiscalYearEnd = companyInformation.FiscalYearEnd;
        StateOfIncorporation = companyInformation.StateOfIncorporation;
        PhoneNumber = companyInformation.PhoneNumber;
        MailingAddress = companyInformation.MailingAddress;
        BusinessAddress = companyInformation.BusinessAddress;
        FormerName = companyInformation.FormerName;
        Industry = companyInformation.Industry;
        DateFounding = companyInformation.DateFounding;
        ChiefExecutiveOfficer = companyInformation.ChiefExecutiveOfficer;
        NumberEmployees = companyInformation.NumberEmployees;
        WebSite = companyInformation.WebSite;
        MarketCap = companyInformation.MarketCap;
        SharesIssued = companyInformation.SharesIssued;
        SharesOutstanding = companyInformation.SharesOutstanding;
        Description = companyInformation.Description;
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

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(Exchange) &&
        !string.IsNullOrWhiteSpace(Symbol);

    public Analysis.Fundamental.CompanyInformation ToDomain()
    {
        return new()
        {
            CentralIndexKey = CentralIndexKey,
            Exchange = Exchange,
            Symbol = Symbol,
            Registrant = Registrant,
            Isin = Isin,
            Lei = Lei,
            Ein = Ein,
            SicCode = SicCode,
            SicDescription = SicDescription,
            SicTitle = SicTitle,
            FiscalYearEnd = FiscalYearEnd,
            StateOfIncorporation = StateOfIncorporation,
            PhoneNumber = PhoneNumber,
            MailingAddress = MailingAddress,
            BusinessAddress = BusinessAddress,
            FormerName = FormerName,
            Industry = Industry,
            DateFounding = DateFounding,
            ChiefExecutiveOfficer = ChiefExecutiveOfficer,
            NumberEmployees = NumberEmployees,
            WebSite = WebSite,
            MarketCap = MarketCap,
            SharesIssued = SharesIssued,
            SharesOutstanding = SharesOutstanding,
            Description = Description,
        };
    }
}