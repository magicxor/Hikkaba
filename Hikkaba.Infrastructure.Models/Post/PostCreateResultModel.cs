using Hikkaba.Infrastructure.Models.Error;
using OneOf;

namespace Hikkaba.Infrastructure.Models.Post;

[GenerateOneOf]
public sealed partial class PostCreateResultModel
    : OneOfBase<long, DomainError>
{
}
