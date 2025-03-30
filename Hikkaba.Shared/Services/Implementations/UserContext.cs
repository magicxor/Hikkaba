using System.Diagnostics.CodeAnalysis;
using Hikkaba.Shared.Exceptions;
using Hikkaba.Shared.Models;
using Hikkaba.Shared.Services.Contracts;

namespace Hikkaba.Shared.Services.Implementations;

public class UserContext : IUserContext
{
    private CurrentUser? _userId;

    /// <summary>
    /// Sets the current user.
    /// </summary>
    /// <param name="user">The current user.</param>
    /// <exception cref="HikkabaDomainException">Thrown when the user is already set.</exception>
    public void SetUser(CurrentUser user)
    {
        if (_userId is not null)
        {
            /*
             Other classes may cache the user id or some user information,
             so we can't allow changing the user id after it was set.
             */
            throw new HikkabaDomainException("User is already set.");
        }

        _userId = user;
    }

    public bool TryGetUser([NotNullWhen(true)] out CurrentUser? user)
    {
        if (_userId is null)
        {
            user = null;
            return false;
        }
        else
        {
            user = _userId;
            return true;
        }
    }

    public CurrentUser? GetUser()
    {
        return _userId;
    }

    public CurrentUser GetRequiredUser()
    {
        return _userId ?? throw new HikkabaDomainException("User is not set.");
    }
}
