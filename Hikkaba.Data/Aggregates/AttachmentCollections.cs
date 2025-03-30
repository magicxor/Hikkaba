using System.Collections.Generic;
using Hikkaba.Data.Entities.Attachments;

namespace Hikkaba.Data.Aggregates;

public class AttachmentCollections
{
    public required ICollection<Audio> Audios { get; set; }
    public required ICollection<Document> Documents { get; set; }
    public required ICollection<Picture> Pictures { get; set; }
    public required ICollection<Video> Videos { get; set; }
}
