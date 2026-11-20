namespace WebApplication1.Domain;

public sealed class LostItem : Item
{
    public LostItem(
        string itemId,
        string name,
        string description,
        string category,
        string locationLost,
        DateTime dateLost,
        DateTime dateReported,
        string reportedByUserId,
        string? imageUrl = null)
        : base(itemId, name, description, category, locationLost, dateReported, reportedByUserId, ItemStatus.Lost, imageUrl)
    {
        DateLost = ValidateDate(dateLost, nameof(dateLost));
        LocationLost = Require(locationLost, nameof(locationLost), 160);
    }

    public DateTime DateLost { get; }
    public string LocationLost { get; }
    public override ItemType Type => ItemType.Lost;

    public override string GetReportSummary() =>
        $"Lost item: {Name}; lost on {DateLost:yyyy-MM-dd} at {LocationLost}.";
}
