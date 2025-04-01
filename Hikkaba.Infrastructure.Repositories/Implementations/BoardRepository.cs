using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.Telemetry;
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
        using var activity = RepositoriesTelemetry.BoardSource.StartActivity();

        return await _applicationDbContext.Boards
            .TagWithCallSite()
            .Select(b => new BoardDetailsModel
            {
                Id = b.Id,
                Name = b.Name,
            })
            .OrderBy(b => b.Id)
            .FirstAsync();
    }

    public async Task EditBoardAsync(string boardName)
    {
        using var activity = RepositoriesTelemetry.BoardSource.StartActivity();

        await _applicationDbContext.Boards
            .TagWithCallSite()
            .ExecuteUpdateAsync(setProp => setProp
                .SetProperty(board => board.Name, boardName));
    }
}
