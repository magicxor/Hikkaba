using Hikkaba.Data.Entities.Attachments.Aggregates;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Infrastructure.Models.Attachments;

namespace Hikkaba.Repositories.Contracts;

public interface IAttachmentRepository
{
    AttachmentCollections ToAttachmentEntities(FileAttachmentCollection inputFiles);
}
