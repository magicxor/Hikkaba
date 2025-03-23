using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Repositories.Implementations;

public sealed class BoardRepository : IBoardRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public BoardRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<BoardRm> GetBoardAsync()
    {
        return await _applicationDbContext.Boards
            .Select(b => new BoardRm
            {
                Id = b.Id,
                Name = b.Name,
            })
            .FirstAsync();
    }

    public async Task EditBoardAsync(string boardName)
    {
        await _applicationDbContext.Boards
            .ExecuteUpdateAsync(setProp => setProp
                .SetProperty(board => board.Name, boardName));
    }
}
