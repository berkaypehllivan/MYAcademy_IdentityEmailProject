using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.Controllers;

public class LoginController:Controller
{
    public IActionResult Login()
    {
        return View();
    }
}
