using System;
using Hikkaba.Common.Dto.Base;

namespace Hikkaba.Common.Dto
{
    public class ThreadDto : BaseMutableDto
    {
        public string Title { get; set; }
        public bool IsPinned { get; set; }
        public bool IsClosed { get; set; }
        public int BumpLimit { get; set; }
        public bool ShowThreadLocalUserHash { get; set; }

        public Guid CategoryId { get; set; }
    }
}