using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.Controllers;

[AllowAnonymous]
public class ErrorPagesController : Controller
{
    public IActionResult Page401()
    {
        var originalPathFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        ViewData["ReturnUrl"] = originalPathFeature?.OriginalPath;
        return View();
    }
    public IActionResult Page403()
    {
        return View();
    }
    public IActionResult Page404()
    {
        return View();
    }
    public IActionResult Page500()
    {
        return View();
    }
}
