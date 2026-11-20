using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Application;
using WebApplication1.Domain;

namespace WebApplication1.Pages.Account;

public class RegisterModel(AuthenticationService authentication) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (Input.Role is UserRole.Staff)
        {
            ModelState.AddModelError(nameof(Input.Role), "Staff accounts must be created by an administrator.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            authentication.Register(Input.Name, Input.Role!.Value, Input.Identifier, Input.Password);
            TempData["Message"] = "Your account was created. You can now sign in.";
            return RedirectToPage("/Account/Login");
        }
        catch (DomainValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }

    public sealed class InputModel
    {
        [Required, StringLength(120, MinimumLength = 2)]
        [Display(Name = "Full name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Account type")]
        public UserRole? Role { get; set; }

        [Required, StringLength(40, MinimumLength = 2)]
        [Display(Name = "Student/faculty number")]
        public string Identifier { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password)), DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}