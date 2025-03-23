using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Enums;

namespace Hikkaba.Data.Entities;

[Table("Bans")]
public class Ban
{
    [Key]
    public int Id { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [Required]
    public required DateTime? EndsAt { get; set; }

    [Required]
    public required IpAddressType IpAddressType { get; set; }

    [Required]
    [Column(TypeName = "varbinary(16)")]
    [MaxLength(Defaults.MaxIpAddressBytesLength)]
    public required byte[] BannedIpAddress { get; set; }

    [Column(TypeName = "varbinary(16)")]
    [MaxLength(Defaults.MaxIpAddressBytesLength)]
    public byte[]? BannedCidrLowerIpAddress { get; set; }

    [Column(TypeName = "varbinary(16)")]
    [MaxLength(Defaults.MaxIpAddressBytesLength)]
    public byte[]? BannedCidrUpperIpAddress { get; set; }

    [MaxLength(Defaults.MaxCountryIsoCodeLength)]
    public string? CountryIsoCode { get; set; }

    public long? AutonomousSystemNumber { get; set; }

    [MaxLength(Defaults.MaxAutonomousSystemOrganizationLength)]
    public string? AutonomousSystemOrganization { get; set; }

    [Required]
    [MaxLength(Defaults.MaxReasonLength)]
    public required string Reason { get; set; }

    // FK id
    [ForeignKey(nameof(RelatedPost))]
    public long? RelatedPostId { get; set; }

    [ForeignKey(nameof(Category))]
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public int CreatedById { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public int? ModifiedById { get; set; }

    // FK models
    public virtual Post? RelatedPost { get; set; }

    public virtual Category? Category { get; set; }

    [Required]
    public virtual ApplicationUser CreatedBy
    {
        get => _createdBy
               ?? throw new InvalidOperationException("Uninitialized property: " + nameof(CreatedBy));
        set => _createdBy = value;
    }

    private ApplicationUser? _createdBy;

    public virtual ApplicationUser? ModifiedBy { get; set; }
}
