using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services
{
    public interface IBoardService
    {
        Task<BoardDto> GetBoardAsync();
    }

    public class BoardService : BaseEntityService, IBoardService
    {
        private readonly ApplicationDbContext _context;
        
        public BoardService(IMapper mapper, ApplicationDbContext context): base(mapper)
        {
            _context = context;
        }
        
        public async Task<BoardDto> GetBoardAsync()
        {
            var entity = await _context.Boards.FirstOrDefaultAsync();
            var dto = MapEntityToDto<BoardDto, Board>(entity);
            return dto;
        }
    }
}