using Hikkaba.Infrastructure.Models.Error;
using OneOf;

namespace Hikkaba.Infrastructure.Models.User;

[GenerateOneOf]
public sealed partial class UserCreateResultModel
    : OneOfBase<UserCreateResultSuccessModel, DomainError>
{
}
