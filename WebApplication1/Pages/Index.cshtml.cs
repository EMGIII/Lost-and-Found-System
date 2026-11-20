using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Application;
using WebApplication1.Domain;

namespace WebApplication1.Pages;

public class IndexModel(LostFoundManager manager, AuthenticationService authentication) : PageModel
{
    public IReadOnlyList<Item> Results { get; private set; } = [];
    public IReadOnlyDictionary<string, User> Reporters { get; private set; } = new Dictionary<string, User>();
    [BindProperty(SupportsGet = true)] public string? Query { get; set; }
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public string? Location { get; set; }
    [BindProperty(SupportsGet = true)] public ItemType? Type { get; set; }

    public void OnGet()
    {
        Results = manager.SearchItems(new SearchCriteria { Query = Query, Category = Category, Location = Location, Type = Type });
        Reporters = authentication.GetAccounts().ToDictionary(account => account.User.UserId, account => account.User);
    }

    public string? GetReporterDetails(Item item)
    {
        if (!Reporters.TryGetValue(item.ReportedByUserId, out var reporter)) return null;

        var identifier = reporter switch
        {
            Student student => $"Student no. {student.StudentNumber}",
            Faculty faculty => $"Faculty no. {faculty.FacultyNumber}",
            WebApplication1.Domain.Staff staff => $"Staff code {staff.StaffCode}",
            _ => null
        };

        return identifier is null ? reporter.Name : $"{reporter.Name} · {identifier}";
    }
}