using System.Diagnostics.CodeAnalysis;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Shared.Models;

namespace Hikkaba.Shared.Services.Contracts;

public interface IUserContext
{
    /// <summary>
    /// Sets the current user.
    /// </summary>
    /// <param name="user">The current user.</param>
    /// <exception cref="HikkabaDomainException">Thrown when the user is already set.</exception>
    void SetUser(CurrentUser user);

    bool TryGetUser([NotNullWhen(true)] out CurrentUser? user);
    CurrentUser? GetUser();
    CurrentUser GetRequiredUser();
}
