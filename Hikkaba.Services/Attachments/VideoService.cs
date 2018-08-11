using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Services.Base.Current;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services.Attachments
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
    }
}