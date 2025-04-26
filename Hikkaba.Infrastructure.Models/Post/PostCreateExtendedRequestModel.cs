namespace Hikkaba.Infrastructure.Models.Post;

public sealed class PostCreateExtendedRequestModel
{
    public required PostCreateRequestModel BaseModel { get; set; }

    // extra fields
    public required byte[] ThreadLocalUserHash { get; set; }
    public required bool IsCyclic { get; set; }
    public required int BumpLimit { get; set; }
    public required int PostCount { get; set; }

    public required ClientInfoModel ClientInfo { get; set; }
}
