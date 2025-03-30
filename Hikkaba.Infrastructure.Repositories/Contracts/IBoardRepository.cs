using Hikkaba.Infrastructure.Models.Board;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IBoardRepository
{
    Task<BoardDetailsModel> GetBoardAsync();
    Task EditBoardAsync(string boardName);
}
