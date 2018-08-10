using System.Linq;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Services.Base.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services.Attachments
{
    public interface IAudioService : IBaseImmutableEntityService<AudioDto, Audio>{}

    public class AudioService: BaseImmutableEntityService<AudioDto, Audio>, IAudioService
    {
        public AudioService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected override DbSet<Audio> GetDbSet(ApplicationDbContext context)
        {
            return context.Audio;
        }
    }
}