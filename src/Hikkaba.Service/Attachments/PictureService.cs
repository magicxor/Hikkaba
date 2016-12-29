using System.Linq;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto.Attachments;
using Hikkaba.Common.Entities.Attachments;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Attachments
{
    public interface IPictureService : IBaseImmutableEntityService<PictureDto, Picture> { }

    public class PictureService : BaseImmutableEntityService<PictureDto, Picture>, IPictureService
    {
        public PictureService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected override DbSet<Picture> GetDbSet(ApplicationDbContext context)
        {
            return context.Pictures;
        }

        protected override IQueryable<Picture> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Pictures.Include(picture => picture.Post);
        }
    }
}