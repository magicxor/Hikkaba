using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Dto.Attachments;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Entities.Attachments;

namespace Hikkaba.Common.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<ApplicationRole, ApplicationRoleDto>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();
            CreateMap<Ban, BanDto>().ReverseMap();
            CreateMap<Board, BoardDto>().ReverseMap();

            // flattening
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.Board, opts => opts.MapFrom(src => src));
            CreateMap<CategoryDto, Board>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.BoardId));

            // flattening
            CreateMap<Post, PostDto>();
            CreateMap<PostDto, Post>()
                .ForMember(dest => dest.Thread, opts => opts.MapFrom(src => src));
            CreateMap<PostDto, Thread>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ThreadId));

            // flattening
            CreateMap<Thread, ThreadDto>();
            CreateMap<ThreadDto, Thread>()
                .ForMember(dest => dest.Category, opts => opts.MapFrom(src => src));
            CreateMap<ThreadDto, Category>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.CategoryId));

            CreateMap<Audio, AudioDto>();
            CreateMap<AudioDto, Audio>()
                .ForMember(dest => dest.Post, opts => opts.MapFrom(src => src));
            CreateMap<AudioDto, Post>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PostId));

            CreateMap<Document, DocumentDto>();
            CreateMap<DocumentDto, Document>()
                .ForMember(dest => dest.Post, opts => opts.MapFrom(src => src));
            CreateMap<DocumentDto, Post>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PostId));

            CreateMap<Notice, NoticeDto>();
            CreateMap<NoticeDto, Notice>()
                .ForMember(dest => dest.Post, opts => opts.MapFrom(src => src))
                .ForMember(dest => dest.Author, opts => opts.MapFrom(src => src));
            CreateMap<NoticeDto, Post>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PostId));
            CreateMap<NoticeDto, ApplicationUser>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.AuthorId));

            CreateMap<Picture, PictureDto>();
            CreateMap<PictureDto, Picture>()
                .ForMember(dest => dest.Post, opts => opts.MapFrom(src => src));
            CreateMap<PictureDto, Post>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PostId));

            CreateMap<Video, VideoDto>();
            CreateMap<VideoDto, Video>()
                .ForMember(dest => dest.Post, opts => opts.MapFrom(src => src));
            CreateMap<VideoDto, Post>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.PostId));

        }
    }
}
