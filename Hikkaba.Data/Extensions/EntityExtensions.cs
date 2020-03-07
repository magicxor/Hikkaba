using TPrimaryKey = System.Guid;
using System.Linq;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Base.Current;

namespace Hikkaba.Data.Extensions
{
    public static class EntityExtensions
    {
        public static TEntity GetLocalOrAttach<TEntity>(this ApplicationDbContext context, TPrimaryKey primaryKey)
            where TEntity : class, IBaseEntity, new()
        {
            return context.Set<TEntity>().Local.FirstOrDefault(e => e.Id == primaryKey) 
                   ?? context.Attach(new TEntity {Id = primaryKey}).Entity;
        }
        
        public static TEntity GetLocal<TEntity>(this ApplicationDbContext context, TPrimaryKey primaryKey)
            where TEntity : class, IBaseEntity, new()
        {
            return context.Set<TEntity>().Local.FirstOrDefault(e => e.Id == primaryKey);
        }
        
        public static bool IsAttached<TEntity>(this ApplicationDbContext context, TPrimaryKey primaryKey)
            where TEntity : class, IBaseEntity, new()
        {
            return context.Set<TEntity>().Local.Any(e => e.Id == primaryKey);
        }
    }
}