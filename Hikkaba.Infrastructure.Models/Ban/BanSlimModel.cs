namespace Hikkaba.Infrastructure.Models.Ban;

public sealed class BanSlimModel
{
    public required int Id { get; set; }

    public required DateTime? EndsAt { get; set; }

    public required string Reason { get; set; }
}
