using Hikkaba.Infrastructure.Models.Ban;

namespace Hikkaba.Application.Contracts;

public interface IBanCreationPrerequisiteService
{
    Task<BanCreationPrerequisites> GetPrerequisitesAsync(long? postId, long threadId, CancellationToken cancellationToken);
}
