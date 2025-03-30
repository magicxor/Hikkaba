using Hikkaba.Data.Aggregates;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IAttachmentRepository
{
    AttachmentCollections ToAttachmentEntities(FileAttachmentContainerCollection inputFiles);
}
