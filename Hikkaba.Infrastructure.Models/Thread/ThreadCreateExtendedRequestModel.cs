using Hikkaba.Infrastructure.Models.Post;

namespace Hikkaba.Infrastructure.Models.Thread;

public sealed class ThreadCreateExtendedRequestModel
{
    public required ThreadCreateRequestModel BaseModel { get; set; }

    // extra fields
    public required Guid ThreadSalt { get; set; }
    public required byte[] ThreadLocalUserHash { get; set; }

    public required ClientInfoModel ClientInfo { get; set; }
}
