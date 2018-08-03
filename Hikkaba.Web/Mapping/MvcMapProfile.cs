using AutoMapper;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Infrastructure.Extensions;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Hikkaba.Web.ViewModels.BansViewModels;
using Hikkaba.Web.ViewModels.BoardViewModels;
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
            CreateMap<BoardDto, BoardViewModel>();
            CreateMap<ApplicationUserDto, ApplicationUserViewModel>();
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
                }).ReverseMap();
            CreateMap<PostDto, PostEditViewModel>().ReverseMap();
            CreateMap<CategoryDto, CategoryViewModel>().ReverseMap();
            CreateMap<BanDto, BanViewModel>().ReverseMap();
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