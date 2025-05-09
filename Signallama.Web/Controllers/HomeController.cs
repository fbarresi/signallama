using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Signallama.Web.Models;
using Signallama.Web.Settings;

namespace Signallama.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly WebAppSettings _settings;

    public HomeController(ILogger<HomeController> logger, WebAppSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public IActionResult Index()
    {
        ViewBag.SignalRBaseAddress = _settings.ChatClientAddress;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}