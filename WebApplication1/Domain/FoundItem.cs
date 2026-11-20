namespace WebApplication1.Domain;

public sealed class FoundItem : Item
{
    public FoundItem(
        string itemId,
        string name,
        string description,
        string category,
        string locationFound,
        DateTime dateFound,
        DateTime dateReported,
        string reportedByUserId,
        string? imageUrl = null)
        : base(itemId, name, description, category, locationFound, dateReported, reportedByUserId, ItemStatus.Found, imageUrl)
    {
        DateFound = ValidateDate(dateFound, nameof(dateFound));
        LocationFound = Require(locationFound, nameof(locationFound), 160);
    }

    public DateTime DateFound { get; }
    public string LocationFound { get; }
    public override ItemType Type => ItemType.Found;

    public override string GetReportSummary() =>
        $"Found item: {Name}; found on {DateFound:yyyy-MM-dd} at {LocationFound}.";
}
