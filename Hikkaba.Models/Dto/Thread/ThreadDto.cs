using Hikkaba.Models.Dto.Base.Current;

namespace Hikkaba.Models.Dto.Thread;

public class ThreadDto : BaseMutableDto
{
    public string Title { get; set; }
    public bool IsPinned { get; set; }
    public bool IsClosed { get; set; }
    public int BumpLimit { get; set; }
    public bool ShowThreadLocalUserHash { get; set; }

    public TPrimaryKey CategoryId { get; set; }
}
