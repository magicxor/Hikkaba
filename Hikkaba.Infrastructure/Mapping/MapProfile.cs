using System;
using AutoMapper;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Attachments;

namespace Hikkaba.Infrastructure.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<DateTime, DateTime>().ConvertUsing<DateTimeToUtcConverter>();
            CreateMap<DateTime?, DateTime?>().ConvertUsing<NullableDateTimeToUtcConverter>();

            CreateMap<ApplicationRole, ApplicationRoleDto>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();

            CreateMap<Ban, BanDto>();
            CreateMap<BanDto, Ban>()
                .ForMember(dest => dest.RelatedPost, opts => opts.Ignore())
                .ForMember(dest => dest.Category, opts => opts.Ignore())
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
            
            CreateMap<Post, PostDto>();
            CreateMap<PostDto, Post>()
                .ForMember(dest => dest.Thread, opts => opts.Ignore())
                .ForMember(dest => dest.Audio, opts => opts.Ignore())
                .ForMember(dest => dest.Documents, opts => opts.Ignore())
                .ForMember(dest => dest.Notices, opts => opts.Ignore())
                .ForMember(dest => dest.Pictures, opts => opts.Ignore())
                .ForMember(dest => dest.Video, opts => opts.Ignore())
                .ForMember(dest => dest.Modified, opts => opts.Ignore())
                .ForMember(dest => dest.ModifiedBy, opts => opts.Ignore())
                .ForMember(dest => dest.Created, opts => opts.Ignore())
                .ForMember(dest => dest.CreatedBy, opts => opts.Ignore());

            CreateMap<Thread, ThreadDto>();
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
            
            CreateMap<Notice, NoticeDto>();
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
}
