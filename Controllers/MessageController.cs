using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.Controllers;

public class MessageController : Controller
{
    private readonly EmailContext _context;
    private readonly UserManager<AppUser> _userManager;

    public MessageController(EmailContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Inbox()
    {
        var values = _context.Messages.ToList();
        return View(values);
    }

    public async Task<IActionResult> SendBox()
    {
        var value = await _userManager.GetUserAsync(User);

        var messages = _context.Messages.Where(x => x.SenderEmail == value.Email).ToList();

        return View(messages);
    }

    public IActionResult CreateMessage()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage(Message message)
    {
        var value = await _userManager.GetUserAsync(User);

        message.SendDate = DateTime.Now;
        message.SenderEmail = value.Email;
        message.CategoryId = 1;

        _context.Messages.Add(message);

        _context.SaveChanges();
        return RedirectToAction("Sendbox");
    }
}