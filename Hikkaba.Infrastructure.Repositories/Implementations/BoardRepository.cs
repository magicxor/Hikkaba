using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class BoardRepository : IBoardRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public BoardRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<BoardDetailsModel> GetBoardAsync()
    {
        return await _applicationDbContext.Boards
            .Select(b => new BoardDetailsModel
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
