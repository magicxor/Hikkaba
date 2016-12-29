using System;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Base
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
