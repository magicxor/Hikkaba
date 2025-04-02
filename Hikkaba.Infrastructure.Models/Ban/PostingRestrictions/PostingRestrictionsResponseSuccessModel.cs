namespace Hikkaba.Infrastructure.Models.Ban;

public class PostingRestrictionsResponseSuccessModel : PostingRestrictionsResponseModel
{
    public required Guid? ThreadSalt { get; set; }
}
