namespace WebApplication1.Domain;

public sealed class Claim
{
    private string _description;

    public Claim(string claimId, string itemId, string claimantId, string description, DateTime submittedDate)
    {
        ClaimId = Require(claimId, nameof(claimId), 50);
        ItemId = Require(itemId, nameof(itemId), 50);
        ClaimantId = Require(claimantId, nameof(claimantId), 50);
        _description = Require(description, nameof(description), 1000);
        SubmittedDate = ValidateDate(submittedDate, nameof(submittedDate));
        Status = ClaimStatus.Pending;
    }

    public string ClaimId { get; }
    public string ItemId { get; }
    public string ClaimantId { get; }
    public string Description { get => _description; private set => _description = Require(value, nameof(Description), 1000); }
    public ClaimStatus Status { get; private set; }
    public DateTime SubmittedDate { get; }
    public string? ReviewedBy { get; private set; }
    public DateTime? ReviewDate { get; private set; }
    public string? RejectionReason { get; private set; }

    public void Submit()
    {
        if (Status != ClaimStatus.Pending)
        {
            throw new DomainValidationException("Only a pending claim can be submitted.");
        }
    }

    public void BeginReview(string reviewerId)
    {
        if (Status != ClaimStatus.Pending)
        {
            throw new DomainValidationException("Only a pending claim can be placed under review.");
        }

        ReviewedBy = Require(reviewerId, nameof(reviewerId), 50);
        ReviewDate = null;
        RejectionReason = null;
        Status = ClaimStatus.UnderReview;
    }

    public void Approve(string reviewerId, DateTime reviewDate)
    {
        Review(ClaimStatus.Approved, reviewerId, reviewDate, null);
    }

    public void Reject(string reviewerId, DateTime reviewDate, string rejectionReason)
    {
        Review(ClaimStatus.Rejected, reviewerId, reviewDate, rejectionReason);
    }

    internal void RestoreReview(ClaimStatus status, string? reviewedBy, DateTime? reviewDate, string? rejectionReason)
    {
        if (status == ClaimStatus.Pending)
        {
            return;
        }

        if (status == ClaimStatus.UnderReview && reviewedBy is not null)
        {
            ReviewedBy = Require(reviewedBy, nameof(reviewedBy), 50);
            Status = ClaimStatus.UnderReview;
            return;
        }

        if (status == ClaimStatus.Approved && reviewedBy is not null && reviewDate.HasValue)
        {
            Approve(reviewedBy, reviewDate.Value);
            return;
        }

        if (status == ClaimStatus.Rejected && reviewedBy is not null && reviewDate.HasValue)
        {
            Reject(reviewedBy, reviewDate.Value, string.IsNullOrWhiteSpace(rejectionReason)
                ? "Legacy record: no rejection reason was recorded."
                : rejectionReason);
            return;
        }

        throw new DomainValidationException("A reviewed claim must contain reviewer and review date information.");
    }

    private void Review(ClaimStatus result, string reviewerId, DateTime reviewDate, string? rejectionReason)
    {
        if (Status is not (ClaimStatus.Pending or ClaimStatus.UnderReview))
        {
            throw new DomainValidationException("Only a pending or under-review claim can be reviewed.");
        }

        RejectionReason = result == ClaimStatus.Rejected
            ? Require(rejectionReason, nameof(rejectionReason), 1000)
            : null;
        ReviewedBy = Require(reviewerId, nameof(reviewerId), 50);
        ReviewDate = ValidateDate(reviewDate, nameof(reviewDate));
        Status = result;
    }

    private static string Require(string? value, string fieldName, int maxLength)
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

    private static DateTime ValidateDate(DateTime value, string fieldName)
    {
        if (value == default || value.Date > DateTime.UtcNow.Date)
        {
            throw new DomainValidationException($"{fieldName} must be a valid date that is not in the future.");
        }

        return value.Date;
    }
}
