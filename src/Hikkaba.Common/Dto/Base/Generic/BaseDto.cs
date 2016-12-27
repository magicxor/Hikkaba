using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Dto.Base
{
    public interface IBaseDto<TPrimaryKey>
    {
        TPrimaryKey Id { get; set; }
    }

    public abstract class BaseDto<TPrimaryKey> : IBaseDto<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
    }
}
