using Hikkaba.Infrastructure.Models.Error;
using OneOf;

namespace Hikkaba.Infrastructure.Models.Thread;

[GenerateOneOf]
public sealed partial class ThreadPostCreateResultModel
    : OneOfBase<ThreadPostCreateSuccessResultModel, DomainError>
{
}
