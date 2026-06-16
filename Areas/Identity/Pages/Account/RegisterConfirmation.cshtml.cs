using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebDispatcher.Areas.Identity.Pages.Account;

public class RegisterConfirmationModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public RegisterConfirmationModel(UserManager<IdentityUser> userManager) => _userManager = userManager;

    public string Email { get; set; } = "";
    public bool DisplayConfirmAccountLink { get; set; }
    public string? EmailConfirmationUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string? email, string? returnUrl = null)
    {
        if (email == null)
            return RedirectToPage("/Index");

        Email = email;
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound($"Unable to load user with email '{email}'.");

        DisplayConfirmAccountLink = false;
        return Page();
    }
}
