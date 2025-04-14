using Hikkaba.Infrastructure.Models.Error;
using OneOf;

namespace Hikkaba.Infrastructure.Models.Role;

[GenerateOneOf]
public sealed partial class RoleCreateResultModel
    : OneOfBase<RoleCreateResultSuccessModel, DomainError>
{
}
