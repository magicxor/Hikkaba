using System.Linq;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Services.Base.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services.Attachments
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
    }
}