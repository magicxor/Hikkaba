using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Generic;

namespace Hikkaba.Data.Entities.Base.Current;

public abstract class BaseMutableEntity : BaseMutableEntity<TPrimaryKey>, IBaseMutableEntity
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public sealed override TPrimaryKey Id { get; set; }

    public sealed override TPrimaryKey GenerateNewId()
    {
        return KeyUtils.GenerateNew();
    }

    protected BaseMutableEntity()
    {
        Id = GenerateNewId();
    }
}