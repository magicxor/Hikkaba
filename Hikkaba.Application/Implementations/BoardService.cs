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

    public async Task<BoardDetailsModel> GetBoardAsync(CancellationToken cancellationToken)
    {
        return await _boardRepository.GetBoardAsync(cancellationToken);
    }

    public async Task EditBoardAsync(string boardName, CancellationToken cancellationToken)
    {
        await _boardRepository.EditBoardAsync(boardName, cancellationToken);
    }
}
