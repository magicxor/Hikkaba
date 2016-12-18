using System.Linq;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto.Attachments;
using Hikkaba.Common.Entities.Attachments;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service.Attachments
{
    public interface IVideoService : IBaseImmutableEntityService<VideoDto, Video> { }

    public class VideoService : BaseImmutableEntityService<VideoDto, Video>, IVideoService
    {
        public VideoService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected override DbSet<Video> GetDbSet(ApplicationDbContext context)
        {
            return context.Video;
        }

        protected override IQueryable<Video> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Video.Include(video => video.Post);
        }

        protected override void LoadReferenceFields(ApplicationDbContext context, Video entityEntry)
        {
            context.Entry(entityEntry).Reference(x => x.Post).Load();
        }
    }
}