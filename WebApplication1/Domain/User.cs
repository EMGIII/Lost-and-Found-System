namespace WebApplication1.Domain;

public abstract class User
{
    private string _name;

    protected User(string userId, string name, UserRole role)
    {
        UserId = Require(userId, nameof(userId), 50);
        _name = Require(name, nameof(name), 120);
        Role = role;
    }

    public string UserId { get; }

    public string Name
    {
        get => _name;
        private set => _name = Require(value, nameof(Name), 120);
    }

    public UserRole Role { get; }

    public string DisplayProfile() => $"{Name} ({Role})";

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
}

public sealed class Student : User
{
    public Student(string userId, string name, string studentNumber)
        : base(userId, name, UserRole.Student)
    {
        StudentNumber = Require(studentNumber, nameof(studentNumber), 40);
    }

    public string StudentNumber { get; }

    public string ReportLostItem() => $"{Name} can report a lost item.";

    public string SubmitClaim() => $"{Name} can submit a claim.";
}

public sealed class Faculty : User
{
    public Faculty(string userId, string name, string facultyNumber)
        : base(userId, name, UserRole.Faculty)
    {
        FacultyNumber = Require(facultyNumber, nameof(facultyNumber), 40);
    }

    public string FacultyNumber { get; }

    public string ReportLostItem() => $"{Name} can report a lost item.";

    public string SubmitClaim() => $"{Name} can submit a claim.";
}

public sealed class Staff : User
{
    public Staff(string userId, string name, string staffCode)
        : base(userId, name, UserRole.Staff)
    {
        StaffCode = Require(staffCode, nameof(staffCode), 40);
    }

    public string StaffCode { get; }

    public string RegisterFoundItem() => $"{Name} can register a found item.";

    public string ReviewClaim() => $"{Name} can review claims.";
}
