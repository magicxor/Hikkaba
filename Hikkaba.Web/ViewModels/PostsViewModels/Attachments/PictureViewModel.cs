using TPrimaryKey = System.Guid;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments
{
    public class PictureViewModel
    {
        public TPrimaryKey Id { get; set; }
        public TPrimaryKey PostId { get; set; }
        public TPrimaryKey ThreadId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
    }
}