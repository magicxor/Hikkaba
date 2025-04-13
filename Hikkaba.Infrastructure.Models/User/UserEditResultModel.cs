using Hikkaba.Infrastructure.Models.Error;
using OneOf;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Models.User;

[GenerateOneOf]
public sealed partial class UserEditResultModel
    : OneOfBase<Success, DomainError>
{
}
