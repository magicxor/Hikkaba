using Hikkaba.Infrastructure.Models.Error;
using OneOf;

namespace Hikkaba.Infrastructure.Models.ApplicationRole;

[GenerateOneOf]
public partial class RoleCreateResultModel
    : OneOfBase<RoleCreateResultSuccessModel, DomainError>
{
}
