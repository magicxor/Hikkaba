using System.IO;

namespace Hikkaba.Common.Dto
{
    public class ThumbnailDto
    {
        public Stream Image { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
