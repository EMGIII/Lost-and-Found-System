namespace WebApplication1.Domain;

public abstract class Item
{
    private string _name;
    private string _description;
    private string _category;
    private string _location;

    protected Item(
        string itemId,
        string name,
        string description,
        string category,
        string location,
        DateTime dateReported,
        string reportedByUserId,
        ItemStatus status,
        string? imageUrl = null)
    {
        ItemId = Require(itemId, nameof(itemId), 50);
        _name = Require(name, nameof(name), 100);
        _description = Require(description, nameof(description), 1000);
        _category = Require(category, nameof(category), 60);
        _location = Require(location, nameof(location), 160);
        DateReported = ValidateDate(dateReported, nameof(dateReported));
        ReportedByUserId = Require(reportedByUserId, nameof(reportedByUserId), 50);
        Status = status;
        ImageUrl = imageUrl;
    }

    public string ItemId { get; }
    public string Name { get => _name; private set => _name = Require(value, nameof(Name), 100); }
    public string Description { get => _description; private set => _description = Require(value, nameof(Description), 1000); }
    public string Category { get => _category; private set => _category = Require(value, nameof(Category), 60); }
    public string Location { get => _location; private set => _location = Require(value, nameof(Location), 160); }
    public DateTime DateReported { get; }
    public string ReportedByUserId { get; }
    public ItemStatus Status { get; private set; }
    public string? ImageUrl { get; }

    public abstract ItemType Type { get; }

    public abstract string GetReportSummary();

    public string DisplayInfo() => $"{Name} - {Category} - {Status}";

    public void UpdateStatus(ItemStatus newStatus)
    {
        if (newStatus == Status)
        {
            return;
        }

        var valid = Type switch
        {
            ItemType.Lost =>
                (Status == ItemStatus.Lost && newStatus == ItemStatus.ClaimPending) ||
                (Status == ItemStatus.ClaimPending && newStatus == ItemStatus.Claimed) ||
                (Status == ItemStatus.Claimed && newStatus == ItemStatus.Returned),
            ItemType.Found =>
                (Status == ItemStatus.Found && newStatus == ItemStatus.ClaimPending) ||
                (Status == ItemStatus.ClaimPending && newStatus == ItemStatus.Claimed) ||
                (Status == ItemStatus.Claimed && newStatus == ItemStatus.Returned),
            _ => false
        };

        if (!valid)
        {
            throw new DomainValidationException($"Cannot change item status from {Status} to {newStatus}.");
        }

        Status = newStatus;
    }

    internal void RestoreStatus(ItemStatus status)
    {
        if (status == Status)
        {
            return;
        }

        Status = status;
    }

    protected static string Require(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"{fieldName} is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainValidationException($"{fieldName} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    protected static DateTime ValidateDate(DateTime value, string fieldName)
    {
        if (value == default || value.Date > DateTime.UtcNow.Date)
        {
            throw new DomainValidationException($"{fieldName} must be a valid date that is not in the future.");
        }

        return value.Date;
    }
}
