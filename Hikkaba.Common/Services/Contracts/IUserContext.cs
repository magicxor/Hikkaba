using System.Diagnostics.CodeAnalysis;
using Hikkaba.Common.Exceptions;
using Hikkaba.Common.Models;

namespace Hikkaba.Common.Services.Contracts;

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
