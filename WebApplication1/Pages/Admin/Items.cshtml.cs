using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Application;
using WebApplication1.Domain;

namespace WebApplication1.Pages.Admin;

[Authorize(Policy = "AdministratorOnly")]
public class ItemsModel(LostFoundManager manager, AuthenticationService authentication) : PageModel
{
    public IReadOnlyList<Item> Items { get; private set; } = [];

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public string? Location { get; set; }
    [BindProperty(SupportsGet = true)] public ItemType? Type { get; set; }
    [BindProperty(SupportsGet = true)] public ItemStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty] public string ItemId { get; set; } = string.Empty;
    [BindProperty] public bool ConfirmReturn { get; set; }

    public void OnGet() => LoadItems();

    public IActionResult OnPostReturn()
    {
        LoadItems();
        if (!ConfirmReturn)
        {
            ModelState.AddModelError(string.Empty, "Confirm that the item was physically returned after identity and ownership verification.");
            return Page();
        }

        var user = authentication.FindUser(User.Identity?.Name ?? string.Empty);
        if (user is not WebApplication1.Domain.Staff staff)
        {
            return Forbid();
        }

        try
        {
            manager.MarkReturned(staff, ItemId);
            TempData["Message"] = "The item was marked as returned and is no longer available for claims.";
            return RedirectToPage(new { Query, Category, Location, Type, Status, DateFrom, DateTo });
        }
        catch (DomainValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }

    private void LoadItems() => Items = manager.SearchItems(new SearchCriteria
    {
        Query = Query,
        Category = Category,
        Location = Location,
        Type = Type,
        Status = Status,
        DateFrom = DateFrom,
        DateTo = DateTo
    });
}
