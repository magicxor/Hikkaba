using TPrimaryKey = System.Guid;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hikkaba.Data.Entities.Base.Generic;

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

    public interface IBaseMutableEntity : IBaseEntity, IBaseMutableEntity<TPrimaryKey> { }
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
}
