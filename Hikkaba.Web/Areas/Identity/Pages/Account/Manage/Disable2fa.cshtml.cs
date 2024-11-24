using System;
using System.Threading.Tasks;
using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Areas.Identity.Pages.Account.Manage;

public class Disable2FaModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<Disable2FaModel> _logger;

    public Disable2FaModel(
        UserManager<ApplicationUser> userManager,
        ILogger<Disable2FaModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            throw new InvalidOperationException($"Cannot disable 2FA for user with ID '{_userManager.GetUserId(User)}' as it's not currently enabled.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var disable2FaResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2FaResult.Succeeded)
        {
            throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{_userManager.GetUserId(User)}'.");
        }

        _logger.LogInformation("User with ID '{UserId}' has disabled 2fa", _userManager.GetUserId(User));
        StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
        return RedirectToPage("./TwoFactorAuthentication");
    }
}
