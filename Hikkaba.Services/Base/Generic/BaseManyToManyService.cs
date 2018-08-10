using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services.Base.Generic
{
    public interface IBaseManyToManyService<TLeftKey, TRightKey>
    {
        Task<bool> AreRelatedAsync(TLeftKey leftId, TRightKey rightId);
        Task CreateAsync(TLeftKey leftId, TRightKey rightId);
        void Delete(TLeftKey leftId, TRightKey rightId);
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
        protected abstract TManyToManyEntity CreateManyToManyEntity(TLeftKey leftId, TRightKey rightId);
        protected abstract TLeftKey GetLeftEntityKey(TManyToManyEntity manyToManyEntity);
        protected abstract TRightKey GetRightEntityKey(TManyToManyEntity manyToManyEntity);

        private bool HasKeys(TManyToManyEntity manyToManyEntity, TLeftKey leftId, TRightKey rightId)
        {
            return ((GetLeftEntityKey(manyToManyEntity).Equals(leftId)) && (GetRightEntityKey(manyToManyEntity).Equals(rightId)));
        }

        public async Task<bool> AreRelatedAsync(TLeftKey leftId, TRightKey rightId)
        {
            return await GetManyToManyDbSet(Context).AnyAsync(x => HasKeys(x, leftId, rightId));
        }

        public async Task CreateAsync(TLeftKey leftId, TRightKey rightId)
        {
            var areRelated = await AreRelatedAsync(leftId, rightId);
            if (!areRelated)
            {
                var manyToManyEntity = CreateManyToManyEntity(leftId, rightId);
                await GetManyToManyDbSet(Context).AddAsync(manyToManyEntity);
            }
        }

        public void Delete(TLeftKey leftId, TRightKey rightId)
        {
            GetManyToManyDbSet(Context)
                .RemoveRange(
                    GetManyToManyDbSet(Context)
                    .Where(manyToManyEntity => HasKeys(manyToManyEntity, leftId, rightId))
                );
        }
    }
}
