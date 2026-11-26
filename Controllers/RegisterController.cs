using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using MYAcademy_IdentityEmailProject.Services;
using System.Security.Cryptography;

namespace MYAcademy_IdentityEmailProject.Controllers;

public class RegisterController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly SignInManager<AppUser> _signInManager;

    private const int ResendCooldown = 30;

    public RegisterController(UserManager<AppUser> userManager, IEmailService emailService, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _emailService = emailService;
        _signInManager = signInManager;
    }

    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(UserRegisterViewModel userRegisterViewModel)
    {
        if (!ModelState.IsValid)
            return View(userRegisterViewModel);

        var validationCode = GenerateVerificationCode();

        var appUser = new AppUser
        {
            Name = userRegisterViewModel.Name,
            Surname = userRegisterViewModel.Surname,
            Email = userRegisterViewModel.Email,
            UserName = userRegisterViewModel.Email,
            ValidCode = validationCode,
            ValidCodeExpireDate = DateTime.Now.AddMinutes(3),
            LastCodeSentDate = DateTime.Now
        };

        var result = await _userManager.CreateAsync(appUser, userRegisterViewModel.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(userRegisterViewModel);
        }

        await _emailService.SendEmailAsync(
            appUser.Email,
            $"{appUser.Name} {appUser.Surname}",
            "Doğrulama Kodu",
            $"Doğrulama kodunuz: {validationCode}");

        return RedirectToAction("VerifyCode", "Register", new { email = appUser.Email });
    }

    private static int GenerateVerificationCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000);
    }

    public async Task<IActionResult> VerifyCode(string email)
    {
        var model = new VerifyCodeViewModel { Email = email };

        var appUser = await _userManager.FindByEmailAsync(email);
        if (appUser?.LastCodeSentDate is not null)
        {
            var elapsed = (DateTime.Now - appUser.LastCodeSentDate.Value).TotalSeconds;
            var remaining = ResendCooldown - elapsed;
            model.ResendCooldown = remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyCode(VerifyCodeViewModel verifyCodeViewModel)
    {
        if (!ModelState.IsValid)
            return View(verifyCodeViewModel);

        var appUser = await _userManager.FindByEmailAsync(verifyCodeViewModel.Email);

        if (appUser is null)
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
            return View(verifyCodeViewModel);
        }

        if (appUser.ValidCodeExpireDate is null || appUser.ValidCodeExpireDate < DateTime.Now)
        {
            ModelState.AddModelError(string.Empty, "Doğrulama kodunuzun süresi dolmuş. Lütfen yeni bir kod oluşturun.");
            return View(verifyCodeViewModel);
        }

        if (appUser.ValidCode.ToString() != verifyCodeViewModel.Code)
        {
            ModelState.AddModelError(string.Empty, "Girilen doğrulama kodu hatalı.");
            return View(verifyCodeViewModel);
        }

        appUser.EmailConfirmed = true;
        appUser.ValidCode = null;
        appUser.ValidCodeExpireDate = null;

        await _userManager.UpdateAsync(appUser);

        await _signInManager.SignInAsync(appUser, isPersistent: false);
        return RedirectToAction("Inbox", "Message");
    }

    [HttpPost]
    public async Task<IActionResult> ResendCode(string email)
    {
        var appUser = await _userManager.FindByEmailAsync(email);

        if (appUser is null)
            return RedirectToAction("SignUp");

        if (appUser.EmailConfirmed)
            return RedirectToAction("SignIn", "Login");

        if (appUser.LastCodeSentDate is not null)
        {
            var elapsed = (DateTime.Now - appUser.LastCodeSentDate.Value).TotalSeconds;
            if (elapsed < ResendCooldown)
            {
                TempData["Error"] = $"Yeni kod isteyebilmek için {Math.Ceiling(ResendCooldown - elapsed)} saniye bekleyin.";
                return RedirectToAction("VerifyCode", new { email });
            }
        }

        var newCode = GenerateVerificationCode();
        appUser.ValidCode = newCode;
        appUser.ValidCodeExpireDate = DateTime.Now.AddMinutes(3);

        await _userManager.UpdateAsync(appUser);

        await _emailService.SendEmailAsync(
            appUser.Email,
            $"{appUser.Name} {appUser.Surname}",
            "Doğrulama Kodu",
            $"Doğrulama kodunuz: {newCode}");

        TempData["Info"] = "Yeni doğrulama kodunuz e-posta adresinize gönderilmiştir.";

        return RedirectToAction("VerifyCode", new { email });

    }
}
