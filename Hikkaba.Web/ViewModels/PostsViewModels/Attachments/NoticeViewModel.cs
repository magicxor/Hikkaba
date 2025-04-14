namespace Hikkaba.Web.ViewModels.PostsViewModels.Attachments;

public sealed class NoticeViewModel
{
    public required long Id { get; set; }
    public required long PostId { get; set; }
    public required long ThreadId { get; set; }
    public required string Text { get; set; }
}
