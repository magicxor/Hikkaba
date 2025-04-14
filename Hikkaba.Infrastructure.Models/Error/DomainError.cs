namespace Hikkaba.Infrastructure.Models.Error;

public sealed class DomainError
{
    public required int StatusCode { get; init; }
    public required string ErrorMessage { get; init; }
}
