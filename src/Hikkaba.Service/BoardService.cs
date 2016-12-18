using System;
using System.Linq;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface IBoardService: IBaseImmutableEntityService<BoardDto, Board> { }

    public class BoardService: BaseImmutableEntityService<BoardDto, Board>, IBoardService
    {
        public BoardService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected override DbSet<Board> GetDbSet(ApplicationDbContext context)
        {
            return context.Boards;
        }

        protected override IQueryable<Board> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Boards;
        }

        protected override void LoadReferenceFields(ApplicationDbContext context, Board entityEntry)
        {
        }
    }
}