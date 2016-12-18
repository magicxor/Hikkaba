using System;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Base
{
    public interface IBaseManyToManyService<TLeftKey, TRightKey>
    {
        Task<bool> AreRelatedAsync(TLeftKey leftEntityId, TRightKey rightEntityId);
        Task CreateAsync(TLeftKey leftEntityId, TRightKey rightEntityId);
        void Delete(TLeftKey leftEntityId, TRightKey rightEntityId);
    }

    public abstract class BaseManyToManyService<TManyToManyEntity, TLeftKey, TRightKey>: IBaseManyToManyService<TLeftKey, TRightKey>
        where TManyToManyEntity : class
    {
        protected ApplicationDbContext Context { get; set; }

        protected BaseManyToManyService(ApplicationDbContext context)
        {
            Context = context;
        }

        protected abstract DbSet<TManyToManyEntity> GetManyToManyDbSet(ApplicationDbContext context);
        protected abstract TManyToManyEntity CreateManyToManyEntity(TLeftKey leftEntity, TRightKey rightEntity);
        protected abstract TLeftKey GetLeftEntityKey(TManyToManyEntity manyToManyEntity);
        protected abstract TRightKey GetRightEntityKey(TManyToManyEntity manyToManyEntity);

        protected bool HasKeys(TManyToManyEntity manyToManyEntity, TLeftKey leftId, TRightKey rightId)
        {
            return ((GetLeftEntityKey(manyToManyEntity).Equals(leftId)) && (GetRightEntityKey(manyToManyEntity).Equals(rightId)));
        }

        public async Task<bool> AreRelatedAsync(TLeftKey leftEntityId, TRightKey rightEntityId)
        {
            return await GetManyToManyDbSet(Context).AnyAsync(x => HasKeys(x, leftEntityId, rightEntityId));
        }

        public async Task CreateAsync(TLeftKey leftEntityId, TRightKey rightEntityId)
        {
            var areRelated = await AreRelatedAsync(leftEntityId, rightEntityId);
            if (!areRelated)
            {
                var manyToManyEntity = CreateManyToManyEntity(leftEntityId, rightEntityId);
                await GetManyToManyDbSet(Context).AddAsync(manyToManyEntity);
            }
        }

        public void Delete(TLeftKey leftEntityId, TRightKey rightEntityId)
        {
            GetManyToManyDbSet(Context)
                .RemoveRange(
                    GetManyToManyDbSet(Context)
                    .Where(manyToManyEntity => HasKeys(manyToManyEntity, leftEntityId, rightEntityId))
                );
        }
    }
}
