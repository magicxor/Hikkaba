using Hikkaba.Infrastructure.Models.Board;

namespace Hikkaba.Services.Contracts;

public interface IBoardService
{
    Task<BoardRm> GetBoardAsync();
    Task EditBoardAsync(string boardName);
}
