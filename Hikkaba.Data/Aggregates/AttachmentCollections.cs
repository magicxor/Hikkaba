using System.Collections.Generic;
using Hikkaba.Data.Entities.Attachments;

namespace Hikkaba.Data.Aggregates;

public class AttachmentCollections
{
    public required ICollection<Audio> Audios { get; init; }
    public required ICollection<Document> Documents { get; init; }
    public required ICollection<Picture> Pictures { get; init; }
    public required ICollection<Video> Videos { get; init; }
}
