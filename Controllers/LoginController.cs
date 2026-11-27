using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Controllers;


[AllowAnonymous]
public class LoginController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;

    public LoginController(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public IActionResult SignIn()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(UserLoginViewModel userLoginViewModel)
    {

        var result = await _signInManager.PasswordSignInAsync(
            userLoginViewModel.Email,
            userLoginViewModel.Password,
            userLoginViewModel.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return RedirectToAction("Inbox", "Message");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Çok fazla başarısız deneme nedeniyle hesabınız geçici olarak kilitlendi. Lütfen birkaç dakika sonra tekrar deneyin.");
            return View(userLoginViewModel);
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız henüz doğrulanmamış. Lütfen e-posta adresinize gönderilen kodu girin.");
            return View(userLoginViewModel);
        }

        ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
        return View(userLoginViewModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SignOut()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("SignIn", "Login");
    }
}