using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Application;
using WebApplication1.Domain;

namespace WebApplication1.Pages.Claims;

[Authorize]
public class MyModel(LostFoundManager manager) : PageModel
{
    public IReadOnlyList<Item> Reports { get; private set; } = [];
    public IReadOnlyList<Claim> Claims { get; private set; } = [];

    public void OnGet()
    {
        var userId = User.Identity!.Name!;
        Reports = manager.Items.Where(item => item.ReportedByUserId == userId).OrderByDescending(item => item.DateReported).ToList();
        Claims = manager.GetClaimsForUser(userId);
    }

    public Item? GetClaimedItem(string itemId)
    {
        try { return manager.GetItem(itemId); }
        catch (DomainValidationException) { return null; }
    }
}
