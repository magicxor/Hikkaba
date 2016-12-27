using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Entities.Base
{
    public interface IBaseEntity<TPrimaryKey>
    {
        TPrimaryKey Id { get; set; }
        TPrimaryKey GenerateNewId();
    }

    public abstract class BaseEntity<TPrimaryKey>: IBaseEntity<TPrimaryKey>
    {
        public abstract TPrimaryKey Id { get; set; }
        public abstract TPrimaryKey GenerateNewId();
    }
}