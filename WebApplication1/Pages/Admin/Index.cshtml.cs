using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Application;
using WebApplication1.Domain;

namespace WebApplication1.Pages.Admin;

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel(LostFoundManager manager, AuthenticationService authentication) : PageModel
{
    public IReadOnlyList<Item> Items { get; private set; } = [];
    public IReadOnlyList<Claim> Claims { get; private set; } = [];
    public int TotalItems => manager.Items.Count;
    public int LostItems => manager.Items.Count(item => item.Type == ItemType.Lost);
    public int FoundItems => manager.Items.Count(item => item.Type == ItemType.Found);
    public int PendingClaims => manager.Claims.Count(claim => claim.Status == WebApplication1.Domain.ClaimStatus.Pending);
    public int UnderReviewClaims => manager.Claims.Count(claim => claim.Status == WebApplication1.Domain.ClaimStatus.UnderReview);
    public int ApprovedClaims => manager.Claims.Count(claim => claim.Status == WebApplication1.Domain.ClaimStatus.Approved);
    public int RejectedClaims => manager.Claims.Count(claim => claim.Status == WebApplication1.Domain.ClaimStatus.Rejected);
    public int ReturnedItems => manager.Items.Count(item => item.Status == ItemStatus.Returned);

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public string? Location { get; set; }
    [BindProperty(SupportsGet = true)] public ItemType? Type { get; set; }
    [BindProperty(SupportsGet = true)] public ItemStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public string? ClaimQuery { get; set; }
    public ClaimStatus? ClaimStatusFilter { get; set; }
    [BindProperty] public string ItemId { get; set; } = string.Empty;
    [BindProperty] public bool ConfirmDelete { get; set; }

    public void OnGet()
    {
        Items = manager.SearchItems(new SearchCriteria
        {
            Query = Query,
            Category = Category,
            Location = Location,
            Type = Type,
            Status = Status,
            DateFrom = DateFrom,
            DateTo = DateTo
        });
        Claims = manager.SearchClaims(ClaimQuery, ClaimStatusFilter);
    }

    public IActionResult OnPostDelete()
    {
        OnGet();
        if (!ConfirmDelete)
        {
            ModelState.AddModelError(string.Empty, "Confirm that you want to permanently remove this item.");
            return Page();
        }

        var user = authentication.FindUser(User.Identity?.Name ?? string.Empty);
        if (user is not WebApplication1.Domain.Staff administrator)
        {
            return Forbid();
        }

        try
        {
            manager.RemoveItem(administrator, ItemId);
            TempData["Message"] = "The item and its linked claim records were removed.";
            return RedirectToPage(new { Query, Category, Location, Type, Status, DateFrom, DateTo, ClaimQuery, ClaimStatusFilter });
        }
        catch (DomainValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
