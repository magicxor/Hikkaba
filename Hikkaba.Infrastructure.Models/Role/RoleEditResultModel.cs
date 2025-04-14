using Hikkaba.Infrastructure.Models.Error;
using OneOf;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Models.Role;

[GenerateOneOf]
public sealed partial class RoleEditResultModel
    : OneOfBase<Success, DomainError>
{
}
