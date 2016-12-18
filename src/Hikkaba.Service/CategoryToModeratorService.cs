using System;
using Hikkaba.Common.Data;
using Hikkaba.Common.Entities;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface ICategoryToModeratorService : IBaseManyToManyService<Guid, Guid> { }

    public class CategoryToModeratorService : BaseManyToManyService<CategoryToModerator, Guid, Guid>, ICategoryToModeratorService
    {
        public CategoryToModeratorService(ApplicationDbContext context) : base(context)
        {
        }


        protected override DbSet<CategoryToModerator> GetManyToManyDbSet(ApplicationDbContext context)
        {
            return Context.CategoriesToModerators;
        }

        protected override CategoryToModerator CreateManyToManyEntity(Guid firstEntity, Guid secondEntity)
        {
            throw new NotImplementedException();
        }

        protected override Guid GetLeftEntityKey(CategoryToModerator manyToManyEntity)
        {
            return manyToManyEntity.CategoryId;
        }

        protected override Guid GetRightEntityKey(CategoryToModerator manyToManyEntity)
        {
            return manyToManyEntity.ApplicationUserId;
        }
    }
}
