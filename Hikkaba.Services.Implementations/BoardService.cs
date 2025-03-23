using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Repositories.Contracts;
using Hikkaba.Services.Contracts;

namespace Hikkaba.Services.Implementations;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;

    public BoardService(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<BoardRm> GetBoardAsync()
    {
        return await _boardRepository.GetBoardAsync();
    }

    public async Task EditBoardAsync(string boardName)
    {
        await _boardRepository.EditBoardAsync(boardName);
    }
}
