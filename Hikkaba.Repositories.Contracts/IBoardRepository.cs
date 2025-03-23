using Hikkaba.Infrastructure.Models.Board;

namespace Hikkaba.Repositories.Contracts;

public interface IBoardRepository
{
    Task<BoardRm> GetBoardAsync();
    Task EditBoardAsync(string boardName);
}
