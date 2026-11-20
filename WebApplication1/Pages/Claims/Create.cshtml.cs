using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Application;
using WebApplication1.Domain;

namespace WebApplication1.Pages.Claims;

[Authorize]
public class CreateModel(LostFoundManager manager, AuthenticationService authentication) : PageModel
{
    public Item? Item { get; private set; }
    [BindProperty(SupportsGet = true)] public string ItemId { get; set; } = string.Empty;
    [BindProperty] public string Description { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        try { Item = manager.GetItem(ItemId); return Page(); } catch (DomainValidationException) { return NotFound(); }
    }

    public IActionResult OnPost()
    {
        try { Item = manager.GetItem(ItemId); } catch (DomainValidationException) { return NotFound(); }
        if (string.IsNullOrWhiteSpace(Description)) ModelState.AddModelError(nameof(Description), "A claim description is required.");
        if (!ModelState.IsValid) return Page();
        var user = authentication.FindUser(User.Identity!.Name!);
        if (user is null) return Challenge();
        try { manager.AddClaim(user, $"CLM-{Guid.NewGuid():N}"[..12].ToUpperInvariant(), ItemId, Description); TempData["Message"] = "Your claim was submitted for staff verification."; return RedirectToPage("/Claims/My"); }
        catch (DomainValidationException exception) { ModelState.AddModelError(string.Empty, exception.Message); return Page(); }
    }
}
