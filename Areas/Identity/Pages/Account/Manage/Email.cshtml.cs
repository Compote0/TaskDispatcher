using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace WebDispatcher.Areas.Identity.Pages.Account.Manage;

public class EmailModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public EmailModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }

    public string? Email { get; set; }
    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "L'e-mail est requis.")]
        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        [Display(Name = "Nouvel e-mail")]
        public string NewEmail { get; set; } = "";
    }

    private async Task LoadAsync(IdentityUser user)
    {
        Email = await _userManager.GetEmailAsync(user);
        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        Input = new InputModel { NewEmail = Email ?? "" };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var email = await _userManager.GetEmailAsync(user);
        if (Input.NewEmail == email)
        {
            StatusMessage = "Votre e-mail est déjà à jour.";
            return RedirectToPage();
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page("/Account/ConfirmEmailChange", pageHandler: null,
            values: new { area = "Identity", userId, email = Input.NewEmail, code }, protocol: Request.Scheme)!;

        await _emailSender.SendEmailAsync(Input.NewEmail, "Confirmez votre nouvel e-mail",
            $"Confirmez votre changement d'e-mail en cliquant ici : <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>lien</a>.");

        StatusMessage = "Un e-mail de confirmation a été envoyé. Vérifiez votre boîte mail.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        Email = await _userManager.GetEmailAsync(user);
        if (Email == null) return NotFound();

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page("/Account/ConfirmEmail", pageHandler: null,
            values: new { area = "Identity", userId, code }, protocol: Request.Scheme)!;

        await _emailSender.SendEmailAsync(Email, "Confirmez votre e-mail",
            $"Confirmez votre compte en cliquant ici : <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>lien</a>.");

        StatusMessage = "E-mail de vérification renvoyé.";
        return RedirectToPage();
    }
}
