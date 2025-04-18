using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Hikkaba.Infrastructure.Repositories.Implementations.Interceptors;

public record ChangedDbItemModel(
    EntityEntry Entry,
    EntityState State,
    IReadOnlyCollection<ChangedDbItemPropModel> ChangedProps
);
