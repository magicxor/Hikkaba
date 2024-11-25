using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto.Board;
using Hikkaba.Services.Contracts;
using Hikkaba.Services.Implementations.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services.Implementations;

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

    public async Task EditBoardAsync(BoardDto dto)
    {
        var existingEntity = await _context.Boards.FirstOrDefaultAsync();
        MapDtoToExistingEntity(dto, existingEntity);
        await _context.SaveChangesAsync();
    }
}
