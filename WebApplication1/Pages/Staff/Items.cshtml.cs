using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Application;
using WebApplication1.Domain;

namespace WebApplication1.Pages.Staff;

[Authorize(Policy = "StaffOnly")]
public class ItemsModel(LostFoundManager manager, AuthenticationService authentication) : PageModel
{
    public IReadOnlyList<Item> Items => manager.Items.OrderByDescending(item => item.DateReported).ToList();

    public IActionResult OnPost(string itemId)
    {
        var user = authentication.FindUser(User.Identity!.Name!);
        if (user is not WebApplication1.Domain.Staff staff) return Forbid();
        try { manager.MarkReturned(staff, itemId); TempData["Message"] = "The item was marked as returned."; return RedirectToPage(); }
        catch (DomainValidationException exception) { ModelState.AddModelError(string.Empty, exception.Message); return Page(); }
    }
}
