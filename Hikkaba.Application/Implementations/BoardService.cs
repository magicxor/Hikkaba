using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Board;
using Hikkaba.Infrastructure.Repositories.Contracts;

namespace Hikkaba.Application.Implementations;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;

    public BoardService(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<BoardDetailsModel> GetBoardAsync()
    {
        return await _boardRepository.GetBoardAsync();
    }

    public async Task EditBoardAsync(string boardName)
    {
        await _boardRepository.EditBoardAsync(boardName);
    }
}
