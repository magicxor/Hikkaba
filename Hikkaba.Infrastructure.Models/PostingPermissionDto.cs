using Hikkaba.Infrastructure.Models.Ban;

namespace Hikkaba.Infrastructure.Models;

public class PostingPermissionDto
{
    public required bool IsPostingAllowed { get; set; }
    public required BanDto Ban { get; set; }
}
