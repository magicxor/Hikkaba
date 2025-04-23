using Hikkaba.Infrastructure.Models.Board;

namespace Hikkaba.Application.Contracts;

public interface IBoardService
{
    Task<BoardDetailsModel> GetBoardAsync(CancellationToken cancellationToken);
}
