using System.Threading.Tasks;
using Hikkaba.Models.Dto.Board;

namespace Hikkaba.Services.Contracts;

public interface IBoardService
{
    Task<BoardDto> GetBoardAsync();
    Task EditBoardAsync(BoardDto dto);
}