using Microsoft.AspNetCore.Identity;
using WebApplication1.Domain;
using WebApplication1.Infrastructure;

namespace WebApplication1.Application;

public sealed class AuthenticationService(JsonUserRepository users, PasswordHasher<User> passwordHasher)
{
    public IReadOnlyList<UserAccount> GetAccounts() => users.Load();

    public User? FindUser(string userId) => users.Load()
        .FirstOrDefault(account => account.User.UserId == userId)?.User;

    public UserAccount? Authenticate(string identifier, string password)
    {
        var account = users.Load().FirstOrDefault(candidate =>
            string.Equals(GetIdentifier(candidate.User), identifier.Trim(), StringComparison.OrdinalIgnoreCase));

        if (account is null)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(account.User, account.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : account;
    }

    public UserAccount Register(string name, UserRole role, string identifier, string password) =>
        users.Add(name, role, identifier, password);

    private static string GetIdentifier(User user) => user switch
    {
        Student student => student.StudentNumber,
        Faculty faculty => faculty.FacultyNumber,
        Staff staff => staff.StaffCode,
        _ => string.Empty
    };
}
