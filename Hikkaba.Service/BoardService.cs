using System.Linq;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
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
    }
}