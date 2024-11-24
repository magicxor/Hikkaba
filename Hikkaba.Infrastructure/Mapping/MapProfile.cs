using System;
using AutoMapper;
using Hikkaba.Data.Aggregations;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Data.Extensions;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Infrastructure.Mapping;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<DateTime, DateTime>().ConvertUsing<DateTimeToUtcConverter>();
        CreateMap<DateTime?, DateTime?>().ConvertUsing<NullableDateTimeToUtcConverter>();

        CreateMap<ApplicationRole, ApplicationRoleDto>().ReverseMap();
        CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();

        CreateMap<Ban, BanDto>();
        CreateMap<BanEditDto, Ban>()
            .ForMember(dest => dest.IsDeleted, opts => opts.Ignore())
            .ForMember(dest => dest.Modified, opts => opts.Ignore())
            .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
            .ForMember(dest => dest.Created, opts => opts.Ignore())
            .ForMember(dest => dest.CreatedBy, opts => opts.Ignore());

        CreateMap<Board, BoardDto>();
        CreateMap<BoardDto, Board>()
            .ForMember(dest => dest.Categories, opts => opts.Ignore());

        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.Board, opts => opts.Ignore())
            .ForMember(dest => dest.Threads, opts => opts.Ignore())
            .ForMember(dest => dest.Moderators, opts => opts.Ignore())
            .ForMember(dest => dest.Modified, opts => opts.Ignore())
            .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
            .ForMember(dest => dest.Created, opts => opts.Ignore())
            .ForMember(dest => dest.CreatedBy, opts => opts.Ignore());
            
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.Audio, opts => opts.MapFrom(post => post.GetAudio()))
            .ForMember(dest => dest.Documents, opts => opts.MapFrom(post => post.GetDocuments()))
            .ForMember(dest => dest.Notices, opts => opts.MapFrom(post => post.GetNotices()))
            .ForMember(dest => dest.Pictures, opts => opts.MapFrom(post => post.GetPictures()))
            .ForMember(dest => dest.Video, opts => opts.MapFrom(post => post.GetVideo()));
        CreateMap<PostDto, Post>()
            .ForMember(dest => dest.Thread, opts => opts.Ignore())
            .ForMember(dest => dest.Attachments, opts => opts.Ignore())
            .ForMember(dest => dest.Modified, opts => opts.Ignore())
            .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
            .ForMember(dest => dest.Created, opts => opts.Ignore())
            .ForMember(dest => dest.CreatedBy, opts => opts.Ignore());
            
        CreateMap<Thread, ThreadDto>();
        CreateMap<ThreadPosts, ThreadAggregationDto>()
            .ForMember(dest => dest.Thread, opts => opts.MapFrom(src => src.Thread))
            .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src.Thread.Category))
            .ForMember(dest => dest.Board, opts => opts.MapFrom(src => src.Thread.Category.Board))
            .ForMember(dest => dest.Posts, opts => opts.MapFrom(src => src.Posts));
            
        CreateMap<ThreadDto, Thread>()
            .ForMember(dest => dest.Category, opts => opts.Ignore())
            .ForMember(dest => dest.Posts, opts => opts.Ignore())
            .ForMember(dest => dest.Modified, opts => opts.Ignore())
            .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
            .ForMember(dest => dest.Created, opts => opts.Ignore())
            .ForMember(dest => dest.CreatedBy, opts => opts.Ignore());

        CreateMap<Audio, AudioDto>();
        CreateMap<AudioDto, Audio>()
            .ForMember(dest => dest.Post, opts => opts.Ignore());
            
        CreateMap<Document, DocumentDto>();
        CreateMap<DocumentDto, Document>()
            .ForMember(dest => dest.Post, opts => opts.Ignore());
            
        CreateMap<Notice, NoticeDto>()
            .ForMember(dest => dest.AuthorName, opts => opts.MapFrom(src => src.Author.UserName));
        CreateMap<NoticeDto, Notice>()
            .ForMember(dest => dest.Post, opts => opts.Ignore())
            .ForMember(dest => dest.Author, opts => opts.Ignore());

        CreateMap<Picture, PictureDto>();
        CreateMap<PictureDto, Picture>()
            .ForMember(dest => dest.Post, opts => opts.Ignore());

        CreateMap<Video, VideoDto>();
        CreateMap<VideoDto, Video>()
            .ForMember(dest => dest.Post, opts => opts.Ignore());
    }
}