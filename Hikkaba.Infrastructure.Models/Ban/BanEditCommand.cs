namespace Hikkaba.Infrastructure.Models.Ban;

public class BanEditCommand
{
    public required int Id { get; set; }

    public required DateTime? EndsAt { get; set; }

    public required string Reason { get; set; }
}
