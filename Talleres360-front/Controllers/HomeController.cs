using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Talleres360_front.Filters;
using Talleres360_front.Models;

namespace Talleres360_front.Controllers;

[AuthRequired]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
