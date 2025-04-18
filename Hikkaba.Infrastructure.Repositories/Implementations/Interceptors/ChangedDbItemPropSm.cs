namespace Hikkaba.Infrastructure.Repositories.Implementations.Interceptors;

public record ChangedDbItemPropSm(
    string Name,
    object? OriginalValue,
    object? CurrentValue
);
