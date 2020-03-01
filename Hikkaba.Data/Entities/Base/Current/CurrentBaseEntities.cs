using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Generic;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Data.Entities.Base.Current
{
    public static class KeyUtils
    {
        public static TPrimaryKey GenerateNew()
        {
            return Guid.NewGuid();
        }
    }

    public interface IBaseEntity : IBaseEntity<TPrimaryKey> { }
    public abstract class BaseEntity : BaseEntity<TPrimaryKey>, IBaseEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override TPrimaryKey Id { get; set; }

        public override TPrimaryKey GenerateNewId()
        {
            return KeyUtils.GenerateNew();
        }

        public BaseEntity()
        {
            Id = GenerateNewId();
        }
    }

    public interface IBaseMutableEntity : IBaseMutableEntity<TPrimaryKey> { }
    public abstract class BaseMutableEntity : BaseMutableEntity<TPrimaryKey>, IBaseMutableEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override TPrimaryKey Id { get; set; }

        public override TPrimaryKey GenerateNewId()
        {
            return KeyUtils.GenerateNew();
        }

        public BaseMutableEntity()
        {
            Id = GenerateNewId();
        }
    }

    public abstract class BaseManyToManyEntity { }
}
