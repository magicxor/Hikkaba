using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Generic;

namespace Hikkaba.Data.Entities.Base.Current;

public abstract class BaseEntity : BaseEntity<TPrimaryKey>, IBaseEntity
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public sealed override TPrimaryKey Id { get; set; }

    public sealed override TPrimaryKey GenerateNewId()
    {
        return KeyUtils.GenerateNew();
    }

    protected BaseEntity()
    {
        Id = GenerateNewId();
    }
}