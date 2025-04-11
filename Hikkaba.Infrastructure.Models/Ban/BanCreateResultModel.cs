using Hikkaba.Infrastructure.Models.Error;
using OneOf;

namespace Hikkaba.Infrastructure.Models.Ban;

[GenerateOneOf]
public partial class BanCreateResultModel
    : OneOfBase<BanCreateResultSuccessModel, DomainError>
{
}
