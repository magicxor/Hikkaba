using Hikkaba.Infrastructure.Models.Error;
using OneOf;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Models.Thread;

[GenerateOneOf]
public sealed partial class ThreadPatchResultModel
    : OneOfBase<Success, DomainError>
{
}
