using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Hikkaba.Infrastructure.Repositories.Implementations.Interceptors;

public record ChangedDbItemSm(
    EntityEntry Entry,
    EntityState State,
    IReadOnlyCollection<ChangedDbItemPropSm> ChangedProps
);
