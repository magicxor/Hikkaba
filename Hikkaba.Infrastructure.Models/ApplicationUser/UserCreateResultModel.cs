using Hikkaba.Infrastructure.Models.Error;
using OneOf;

namespace Hikkaba.Infrastructure.Models.ApplicationUser;

[GenerateOneOf]
public sealed partial class UserCreateResultModel
    : OneOfBase<UserCreateResultSuccessModel, DomainError>
{
}
