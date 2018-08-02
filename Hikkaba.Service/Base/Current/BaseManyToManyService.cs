using System;
using Hikkaba.Data.Context;
using Hikkaba.Service.Base.Generic;

namespace Hikkaba.Service.Base.Current
{
    public interface IBaseManyToManyService: IBaseManyToManyService<Guid, Guid> {}

    public abstract class BaseManyToManyService<TManyToManyEntity>: BaseManyToManyService<TManyToManyEntity, Guid, Guid>, IBaseManyToManyService
        where TManyToManyEntity : class
    {
        public BaseManyToManyService(ApplicationDbContext context) : base(context)
        {
        }
    }
}
