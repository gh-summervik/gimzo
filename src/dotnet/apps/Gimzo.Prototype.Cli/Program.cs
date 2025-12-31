using OoplesFinance.YahooFinanceAPI;
using OoplesFinance.YahooFinanceAPI.Enums;

var yahooClient = new YahooClient();
string ticker = "AAPL";  // Replace with your symbol

// Fetch recent EOD data (last 2 days by default; adjust TimeRange as needed)
var historicalData = await yahooClient.GetHistoricalDataAsync(ticker,
    DataFrequency.Daily, DateTime.Now.AddDays(-2), DateTime.Now);

foreach (var data in historicalData)
{
    Console.WriteLine($"Date: {data.Date:yyyy-MM-dd}, Open: {data.Open}, High: {data.High}, " +
                      $"Low: {data.Low}, Close: {data.Close}, Volume: {data.Volume}, " +
                      $"Adjusted Close: {data.AdjustedClose}");
}