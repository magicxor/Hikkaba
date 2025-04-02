using Hikkaba.Shared.Enums;

namespace Hikkaba.Infrastructure.Models.Ban;

public class BanCreateCommand
{
    public required DateTime? EndsAt { get; set; }

    public required string BannedIpAddress { get; set; }

    public required bool BanByNetwork { get; set; }

    public required bool BanInAllCategories { get; set; }

    public required BanAdditionalAction AdditionalAction { get; set; }

    public required string Reason { get; set; }

    public required long? RelatedPostId { get; set; }

    public required long RelatedThreadId { get; set; }

    public required string? CategoryAlias { get; set; }
}
