namespace Hikkaba.Infrastructure.Repositories.Implementations.Interceptors;

public record ChangedDbItemPropModel(
    string Name,
    object? OriginalValue,
    object? CurrentValue
);
