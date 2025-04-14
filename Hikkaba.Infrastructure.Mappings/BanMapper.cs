using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Riok.Mapperly.Abstractions;

namespace Hikkaba.Infrastructure.Mappings;

[Mapper]
public static partial class BanMapper
{
    [MapProperty(nameof(Ban.Id), nameof(BanSlimModel.Id))]
    [MapProperty(nameof(Ban.EndsAt), nameof(BanSlimModel.EndsAt))]
    [MapProperty(nameof(Ban.Reason), nameof(BanSlimModel.Reason))]
    [MapperIgnoreSource(nameof(Ban.IsDeleted))]
    [MapperIgnoreSource(nameof(Ban.CreatedAt))]
    [MapperIgnoreSource(nameof(Ban.ModifiedAt))]
    [MapperIgnoreSource(nameof(Ban.IpAddressType))]
    [MapperIgnoreSource(nameof(Ban.BannedIpAddress))]
    [MapperIgnoreSource(nameof(Ban.BannedCidrLowerIpAddress))]
    [MapperIgnoreSource(nameof(Ban.BannedCidrUpperIpAddress))]
    [MapperIgnoreSource(nameof(Ban.CountryIsoCode))]
    [MapperIgnoreSource(nameof(Ban.AutonomousSystemNumber))]
    [MapperIgnoreSource(nameof(Ban.AutonomousSystemOrganization))]
    [MapperIgnoreSource(nameof(Ban.RelatedPostId))]
    [MapperIgnoreSource(nameof(Ban.CategoryId))]
    [MapperIgnoreSource(nameof(Ban.CreatedById))]
    [MapperIgnoreSource(nameof(Ban.ModifiedById))]
    [MapperIgnoreSource(nameof(Ban.RelatedPost))]
    [MapperIgnoreSource(nameof(Ban.Category))]
    [MapperIgnoreSource(nameof(Ban.CreatedBy))]
    [MapperIgnoreSource(nameof(Ban.ModifiedBy))]
    public static partial BanSlimModel ToPreview(this Ban entity);
}
