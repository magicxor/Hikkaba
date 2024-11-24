using TPrimaryKey = System.Guid;

namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments;

public class NoticeViewModel
{
    public TPrimaryKey Id { get; set; }
    public TPrimaryKey PostId { get; set; }
    public TPrimaryKey ThreadId { get; set; }
    public string Text { get; set; }
    public TPrimaryKey AuthorId { get; set; }
    public string AuthorName { get; set; }
}