namespace Hikkaba.Infrastructure.Models.Ban.PostingRestrictions;

public class PostingRestrictionsResponseSuccessModel : PostingRestrictionsResponseModel
{
    public required Guid? ThreadSalt { get; set; }
    public required bool IsClosed { get; set; }
    public required bool IsCyclic { get; set; }
    public required int BumpLimit { get; set; }
    public required int PostCount { get; set; }
}
