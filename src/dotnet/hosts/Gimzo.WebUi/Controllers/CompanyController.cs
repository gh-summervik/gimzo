using Gimzo.AppServices.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Gimzo.WebUi.Controllers;

public class CompanyController(UiModelService modelService, ILogger<CompanyController> logger) : Controller
{
    private readonly UiModelService _modelService = modelService;
    private readonly ILogger<CompanyController> _logger = logger;

    [HttpGet("/company-info/{symbol}")]
    public async Task<IActionResult> GetCompanyInfo([FromRoute] string symbol)
    {
        CompanyInfo? model = await _modelService.GetCompanyInfoAsync(symbol.ToUpperInvariant());
        return View("CompanyInfo", model);
    }

    [HttpGet("/company-info/{symbol}/chart")]
    public async Task<IActionResult> GetCompanyChart([FromRoute] string symbol,
        [FromQuery] string scale = "1y")
    {
        scale ??= "1y";
        scale = scale.Trim().ToLowerInvariant();

        int factor = 0;

        const string pattern = @"(\d+)([w|m|y])";
        if (Regex.IsMatch(scale, pattern))
        {
            MatchCollection matches = Regex.Matches(scale, pattern);
            var num = Math.Max(1, Convert.ToInt32(matches[0].Groups[1].Value));
            var f = matches[0].Groups[2].Value switch
            {
                "w" => 7,
                "m" => 28,
                _ => 365
            };
            factor = num * f;
        }

        DateOnly start = DateOnly.MinValue, finish = DateOnly.FromDateTime(DateTime.Now);

        if (factor > 0)
        {
            start = scale == "max" ? new DateOnly(1970, 1, 1) : DateOnly.FromDateTime(DateTime.Now.AddDays(-1 * factor));
        }

        ChartModel? model = await _modelService.GetChartModelAsync(symbol.ToUpperInvariant(), start, finish);
        return View("Chart", model);
    }
}