namespace Hikkaba.Infrastructure.Models.Thread;

public class ThreadCreateExtendedRequestModel
{
    public required ThreadCreateRequestModel BaseModel { get; set; }

    // extra fields
    public required Guid ThreadSalt { get; set; }
    public required byte[] ThreadLocalUserHash { get; set; }
}
