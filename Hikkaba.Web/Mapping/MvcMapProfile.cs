using TPrimaryKey = System.Guid;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                });
            CreateMap<PostDetailsViewModel, PostDto>()
                .ForMember(dest => dest.CreatedBy, opts => opts.Ignore())
                .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
                .ForMember(dest => dest.Audio, opts => opts.Ignore())
                .ForMember(dest => dest.Documents, opts => opts.Ignore())
                .ForMember(dest => dest.Notices, opts => opts.Ignore())
                .ForMember(dest => dest.Pictures, opts => opts.Ignore())
                .ForMember(dest => dest.Video, opts => opts.Ignore());
            CreateMap<PostDto, PostEditViewModel>()
                .ForMember(dest => dest.CategoryAlias, opts => opts.Ignore())
                .ReverseMap();
            CreateMap<CategoryDto, CategoryViewModel>()
                .ReverseMap();
            CreateMap<BanDto, BanDetailsViewModel>();
            CreateMap<BanDto, BanEditViewModel>();
            CreateMap<BanEditViewModel, BanEditDto>();
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
            CreateMap<ThreadAggregationDto, ThreadDetailsViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Thread.Id))
                .ForMember(dest => dest.IsDeleted, opts => opts.MapFrom(src => src.Thread.IsDeleted))
                .ForMember(dest => dest.Created, opts => opts.MapFrom(src => src.Thread.Created))
                .ForMember(dest => dest.Modified, opts => opts.MapFrom(src => src.Thread.Modified))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.Thread.Title))
                .ForMember(dest => dest.IsPinned, opts => opts.MapFrom(src => src.Thread.IsPinned))
                .ForMember(dest => dest.IsClosed, opts => opts.MapFrom(src => src.Thread.IsClosed))
                .ForMember(dest => dest.BumpLimit, opts => opts.MapFrom(src => src.Thread.BumpLimit))
                .ForMember(dest => dest.ShowThreadLocalUserHash, opts => opts.MapFrom(src => src.Thread.ShowThreadLocalUserHash))
                .ForMember(dest => dest.CategoryId, opts => opts.MapFrom(src => src.Thread.CategoryId))
                .ForMember(dest => dest.CategoryAlias, opts => opts.MapFrom(src => src.Category.Alias))
                .ForMember(dest => dest.CategoryName, opts => opts.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.PostCount, opts => opts.MapFrom(src => src.Posts.Count))
                .ForMember(dest => dest.Posts, opts => opts.MapFrom(src => src.Posts))
                .AfterMap((src, dest) =>
                {
                    dest.Posts.ForEach((index, destElement) =>
                    {
                        destElement.Index = index;
                        destElement.ThreadShowThreadLocalUserHash = src.Thread.ShowThreadLocalUserHash;
                        destElement.CategoryAlias = src.Category.Alias;
                        destElement.CategoryId = src.Category.Id;
                        destElement.Answers = new List<TPrimaryKey>(src.Posts
                            .Where(answer => Regex.IsMatch(answer.Message, $@">>{destElement.Id}(?![\w])"))
                            .Select(answer => answer.Id))
                            .ToList();
                    });
                });
        }
    }
}