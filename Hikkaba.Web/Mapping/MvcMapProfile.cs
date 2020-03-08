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
            CreateMap<ThreadDto, ThreadDetailsViewModel>()
                .ForMember(dest => dest.CategoryAlias, opts => opts.Ignore())
                .ForMember(dest => dest.CategoryName, opts => opts.Ignore())
                .ForMember(dest => dest.PostCount, opts => opts.Ignore())
                .ForMember(dest => dest.Posts, opts => opts.Ignore());
            CreateMap<ThreadDto, ThreadEditViewModel>()
                .ForMember(dest => dest.CategoryAlias, opts => opts.Ignore())
                .ReverseMap();
            CreateMap<PostDto, PostDetailsViewModel>()
                .ForMember(dest => dest.Index, opts => opts.Ignore())
                .ForMember(dest => dest.ThreadShowThreadLocalUserHash, opts => opts.Ignore())
                .ForMember(dest => dest.CategoryAlias, opts => opts.Ignore())
                .ForMember(dest => dest.CategoryId, opts => opts.Ignore())
                .ForMember(dest => dest.Answers, opts => opts.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.Audio.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                    dest.Documents.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                    dest.Notices.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                    dest.Pictures.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                    dest.Video.ForEach(destElement => destElement.ThreadId = src.ThreadId);
                })
                .ReverseMap();
            CreateMap<PostDto, PostEditViewModel>()
                .ForMember(dest => dest.CategoryAlias, opts => opts.Ignore())
                .ReverseMap();
            CreateMap<CategoryDto, CategoryViewModel>()
                .ReverseMap();
            CreateMap<BanDto, BanViewModel>()
                .ReverseMap();
            CreateMap<AudioDto, AudioViewModel>()
                .ForMember(dest => dest.ThreadId, opts => opts.Ignore());
            CreateMap<DocumentDto, DocumentViewModel>()
                .ForMember(dest => dest.ThreadId, opts => opts.Ignore());
            CreateMap<NoticeDto, NoticeViewModel>()
                .ForMember(dest => dest.ThreadId, opts => opts.Ignore());
            CreateMap<PictureDto, PictureViewModel>()
                .ForMember(dest => dest.ThreadId, opts => opts.Ignore());
            CreateMap<VideoDto, VideoViewModel>()
                .ForMember(dest => dest.ThreadId, opts => opts.Ignore());

            CreateMap<PostAnonymousCreateViewModel, PostDto>()
                .ForMember(dest => dest.UserIpAddress, opts => opts.Ignore())
                .ForMember(dest => dest.UserAgent, opts => opts.Ignore())
                .ForMember(dest => dest.Audio, opts => opts.Ignore())
                .ForMember(dest => dest.Documents, opts => opts.Ignore())
                .ForMember(dest => dest.Notices, opts => opts.Ignore())
                .ForMember(dest => dest.Pictures, opts => opts.Ignore())
                .ForMember(dest => dest.Video, opts => opts.Ignore())
                .ForMember(dest => dest.IsDeleted, opts => opts.Ignore())
                .ForMember(dest => dest.Created, opts => opts.Ignore())
                .ForMember(dest => dest.Modified, opts => opts.Ignore())
                .ForMember(dest => dest.CreatedBy, opts => opts.Ignore())
                .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
                .ForMember(dest => dest.Id, opts => opts.Ignore());
            CreateMap<ThreadAnonymousCreateViewModel, PostDto>()
                .ForMember(dest => dest.IsSageEnabled, opts => opts.Ignore())
                .ForMember(dest => dest.UserIpAddress, opts => opts.Ignore())
                .ForMember(dest => dest.UserAgent, opts => opts.Ignore())
                .ForMember(dest => dest.Audio, opts => opts.Ignore())
                .ForMember(dest => dest.Documents, opts => opts.Ignore())
                .ForMember(dest => dest.Notices, opts => opts.Ignore())
                .ForMember(dest => dest.Pictures, opts => opts.Ignore())
                .ForMember(dest => dest.Video, opts => opts.Ignore())
                .ForMember(dest => dest.ThreadId, opts => opts.Ignore())
                .ForMember(dest => dest.IsDeleted, opts => opts.Ignore())
                .ForMember(dest => dest.Created, opts => opts.Ignore())
                .ForMember(dest => dest.Modified, opts => opts.Ignore())
                .ForMember(dest => dest.CreatedBy, opts => opts.Ignore())
                .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
                .ForMember(dest => dest.Id, opts => opts.Ignore());
            CreateMap<ThreadAnonymousCreateViewModel, ThreadDto>()
                .ForMember(dest => dest.IsPinned, opts => opts.Ignore())
                .ForMember(dest => dest.IsClosed, opts => opts.Ignore())
                .ForMember(dest => dest.BumpLimit, opts => opts.Ignore())
                .ForMember(dest => dest.ShowThreadLocalUserHash, opts => opts.Ignore())
                .ForMember(dest => dest.CategoryId, opts => opts.Ignore())
                .ForMember(dest => dest.IsDeleted, opts => opts.Ignore())
                .ForMember(dest => dest.Created, opts => opts.Ignore())
                .ForMember(dest => dest.Modified, opts => opts.Ignore())
                .ForMember(dest => dest.CreatedBy, opts => opts.Ignore())
                .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
                .ForMember(dest => dest.Id, opts => opts.Ignore());
        }
    }
}