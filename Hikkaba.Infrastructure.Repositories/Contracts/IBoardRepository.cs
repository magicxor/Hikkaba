using Hikkaba.Infrastructure.Models.Board;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IBoardRepository
{
    Task<BoardDetailsModel> GetBoardAsync(CancellationToken cancellationToken);
    Task EditBoardAsync(string boardName, CancellationToken cancellationToken);
}
