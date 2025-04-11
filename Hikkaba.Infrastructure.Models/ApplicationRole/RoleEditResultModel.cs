using Hikkaba.Infrastructure.Models.Error;
using OneOf;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Models.ApplicationRole;

[GenerateOneOf]
public sealed partial class RoleEditResultModel
    : OneOfBase<Success, DomainError>
{
}
