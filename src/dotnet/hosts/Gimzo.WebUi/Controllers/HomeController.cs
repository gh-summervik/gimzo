using Gimzo.AppServices.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gimzo.WebUi.Controllers;

public class HomeController(UiModelService modelService, ILogger<HomeController> logger) : Controller
{
    private readonly UiModelService _dataService = modelService;
    private readonly ILogger<HomeController> _logger = logger;

    [HttpGet("/")]
    public IActionResult GetIndex()
    {
        return View("Index");
    }

    [HttpPost("/search")]
    public async Task<IActionResult> SearchTicker(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            return BadRequest("Ticker required.");

        return Redirect($"/company-info/{ticker.ToUpperInvariant()}");
    }

    [HttpGet("/company-info/{ticker}")]
    public async Task<IActionResult> GetCompanyInfo(string ticker)
    {
        CompanyInfo? model = await _dataService.GetCompanyInfoAsync(ticker.ToUpperInvariant());
        return View("CompanyInfo", model);
    }


}