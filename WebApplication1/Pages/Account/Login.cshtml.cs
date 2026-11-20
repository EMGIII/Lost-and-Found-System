using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Application;
using AppAuthenticationService = WebApplication1.Application.AuthenticationService;

namespace WebApplication1.Pages.Account;

public class LoginModel(AppAuthenticationService authentication) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var account = authentication.Authenticate(Input.Identifier, Input.Password);
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "The student, faculty, or staff number or password is not correct.");
            return Page();
        }

        var claims = new[] { new System.Security.Claims.Claim(ClaimTypes.Name, account.User.UserId), new System.Security.Claims.Claim(ClaimTypes.Role, account.User.Role.ToString()), new System.Security.Claims.Claim("DisplayName", account.User.Name) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return LocalRedirect(string.IsNullOrWhiteSpace(ReturnUrl) ? "/" : ReturnUrl);
    }

    public sealed class InputModel
    {
        [Required, Display(Name = "Student/faculty/staff number")]
        public string Identifier { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    }
}
