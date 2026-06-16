using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace WebDispatcher.Areas.Identity.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public ConfirmEmailModel(UserManager<IdentityUser> userManager) => _userManager = userManager;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code, string? returnUrl = null)
    {
        if (userId == null || code == null)
            return RedirectToPage("/Index");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            StatusMessage = "Erreur : utilisateur introuvable.";
            return Page();
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        StatusMessage = result.Succeeded
            ? "Merci ! Votre e-mail a été confirmé. Vous pouvez vous connecter."
            : "Erreur lors de la confirmation. Le lien est peut-être expiré.";
        return Page();
    }
}
