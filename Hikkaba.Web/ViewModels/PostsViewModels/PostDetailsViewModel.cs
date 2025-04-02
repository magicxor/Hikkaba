using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Hikkaba.Web.ViewModels.PostsViewModels.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels;

public class PostDetailsViewModel
{
    public required int Index { get; set; }

    [Display(Name = @"Post id")]
    public required long Id { get; set; }

    [Display(Name = @"Is deleted")]
    public required bool IsDeleted { get; set; }

    [Display(Name = @"Created at")]
    public required DateTime CreatedAt { get; set; }

    [Display(Name = @"Modified at")]
    public required DateTime? ModifiedAt { get; set; }

    [Display(Name = @"Sage")]
    public required bool IsSageEnabled { get; set; }

    [Display(Name = @"Message")]
    public required string MessageHtml { get; set; }

    [Display(Name = @"IP")]
    public required IPAddress? UserIpAddress { get; set; }

    [Display(Name = @"User-agent")]
    public required string UserAgent { get; set; }

    [Display(Name = @"Audio")]
    public required IReadOnlyList<AudioViewModel> Audio { get; set; }

    [Display(Name = @"Documents")]
    public required IReadOnlyList<DocumentViewModel> Documents { get; set; }

    [Display(Name = @"Notices")]
    public required IReadOnlyList<NoticeViewModel> Notices { get; set; }

    [Display(Name = @"Pictures")]
    public required IReadOnlyList<PictureViewModel> Pictures { get; set; }

    [Display(Name = @"Video")]
    public required IReadOnlyList<VideoViewModel> Video { get; set; }

    public required long ThreadId { get; set; }

    [Display(Name = @"Show thread-local user hashes")]
    public required bool ShowThreadLocalUserHash { get; set; }

    [Display(Name = @"Thread-local user hash")]
    public required string ThreadLocalUserHash { get; set; }

    [Display(Name = @"Category")]
    public required string CategoryAlias { get; set; }

    public required int CategoryId { get; set; }

    [Display(Name = @"Replies")]
    public required IReadOnlyList<long> Replies { get; set; }
}
