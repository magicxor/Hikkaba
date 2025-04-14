using Hikkaba.Infrastructure.Models.Error;
using OneOf;

namespace Hikkaba.Infrastructure.Models.Ban;

[GenerateOneOf]
public sealed partial class BanCreateResultModel
    : OneOfBase<BanCreateResultSuccessModel, DomainError>
{
}
