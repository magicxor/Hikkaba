using Hikkaba.Infrastructure.Models.Error;
using OneOf;
using OneOf.Types;

namespace Hikkaba.Infrastructure.Models.ApplicationRole;

[GenerateOneOf]
public partial class RoleDeleteResultModel
    : OneOfBase<Success, DomainError>
{
}
