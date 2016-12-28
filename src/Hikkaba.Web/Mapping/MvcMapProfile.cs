using AutoMapper;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Dto.Attachments;
using Hikkaba.Common.Extensions;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels.Attachments;
using Hikkaba.Web.ViewModels.ThreadsViewModels;

namespace Hikkaba.Web.Mapping
{
    public class MvcMapProfile : Profile
    {
        public MvcMapProfile()
        {
            CreateMap<ThreadDto, ThreadDetailsViewModel>();
            CreateMap<ThreadDto, ThreadEditViewModel>().ReverseMap();
            CreateMap<PostDto, PostDetailsViewModel>()
                .AfterMap((src, dest) =>
                {
                    dest.Audio.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                    dest.Documents.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                    dest.Notices.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                    dest.Pictures.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                    dest.Video.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                });
            CreateMap<PostDto, PostEditViewModel>().ReverseMap();
            CreateMap<CategoryDto, CategoryViewModel>();
            CreateMap<AudioDto, AudioViewModel>();
            CreateMap<DocumentDto, DocumentViewModel>();
            CreateMap<NoticeDto, NoticeViewModel>();
            CreateMap<PictureDto, PictureViewModel>();
            CreateMap<VideoDto, VideoViewModel>();

            CreateMap<PostAnonymousCreateViewModel, PostDto>();
            CreateMap<ThreadAnonymousCreateViewModel, PostDto>();
            CreateMap<ThreadAnonymousCreateViewModel, ThreadDto>();
        }
    }
}