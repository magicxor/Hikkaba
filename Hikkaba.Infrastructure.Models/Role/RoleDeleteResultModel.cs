using Hikkaba.Infrastructure.Models.Error;
using OneOf;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Models.Role;

[GenerateOneOf]
public sealed partial class RoleDeleteResultModel
    : OneOfBase<Success, DomainError>
{
}
