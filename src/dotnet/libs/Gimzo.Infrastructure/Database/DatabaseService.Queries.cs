using Dapper;

namespace Gimzo.Infrastructure.Database;

internal sealed partial class DatabaseService
{
    public async Task<Analysis.Fundamental.CompanyInformation?> GetCompanyInformationAsync(string symbol)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));

        using var queryCtx = _dbDefPair.GetQueryConnection();
        string sql = $"{SqlRepository.SelectCompanyInfo} WHERE symbol = @Symbol";
        var dao = queryCtx.QueryFirstOrDefault<DataAccessObjects.CompanyInformation>(sql, new { Symbol = symbol.ToUpperInvariant() });
        return dao?.ToDomain();
    }

    public async Task<Analysis.Technical.Charts.Ohlc[]> GetOhlcAsync(string symbol)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));
        using var queryCtx = _dbDefPair.GetQueryConnection();
        string sql = $"{SqlRepository.SelectEodPrices} WHERE symbol = @Symbol";
        var daos = (await queryCtx.QueryAsync<DataAccessObjects.EodPrice>(sql,
            new { Symbol = symbol.ToUpperInvariant() })).OrderBy(k => k.Date).ToArray();
        
        if (daos.Length == 0)
            return [];

        return [.. daos.Select(k => k.ToOhlc())];
    }
}