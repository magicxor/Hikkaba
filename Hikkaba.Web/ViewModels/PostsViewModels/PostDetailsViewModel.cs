using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Hikkaba.Web.ViewModels.PostsViewModels.Attachments;

namespace Hikkaba.Web.ViewModels.PostsViewModels;

public sealed record PostDetailsViewModel
{
    public required int Index { get; init; }

    [Display(Name = @"Post id")]
    public required long Id { get; init; }

    [Display(Name = @"Is deleted")]
    public required bool IsDeleted { get; init; }

    [Display(Name = @"Created at")]
    public required DateTime CreatedAt { get; init; }

    [Display(Name = @"Modified at")]
    public required DateTime? ModifiedAt { get; init; }

    [Display(Name = @"Sage")]
    public required bool IsSageEnabled { get; init; }

    [Display(Name = @"Message")]
    public required string MessageHtml { get; init; }

    [Display(Name = @"IP")]
    public required IPAddress? UserIpAddress { get; init; }

    [Display(Name = @"User-agent")]
    public required string UserAgent { get; init; }

    [Display(Name = @"Audio")]
    public required IReadOnlyList<AudioViewModel> Audio { get; init; }

    [Display(Name = @"Documents")]
    public required IReadOnlyList<DocumentViewModel> Documents { get; init; }

    [Display(Name = @"Notices")]
    public required IReadOnlyList<NoticeViewModel> Notices { get; init; }

    [Display(Name = @"Pictures")]
    public required IReadOnlyList<PictureViewModel> Pictures { get; init; }

    [Display(Name = @"Video")]
    public required IReadOnlyList<VideoViewModel> Video { get; init; }

    public required long ThreadId { get; init; }

    [Display(Name = @"Show thread-local user hashes")]
    public required bool ShowThreadLocalUserHash { get; init; }

    [Display(Name = @"Show user OS")]
    public required bool ShowOs { get; init; }

    [Display(Name = @"Show user browser")]
    public required bool ShowBrowser { get; init; }

    [Display(Name = @"Show user country")]
    public required bool ShowCountry { get; init; }

    [Display(Name = @"Show category alias")]
    public bool ShowCategoryAlias { get; init; }

    [Display(Name = @"Thread-local user hash")]
    public required string ThreadLocalUserHash { get; init; }

    [Display(Name = @"Category")]
    public required string CategoryAlias { get; init; }

    public required int CategoryId { get; init; }

    [Display(Name = @"Replies")]
    public required IReadOnlyList<long> Replies { get; init; }

    // [Display(Name = @"User was banned")]
    // public required bool IsBannedForThisPost { get; init; } /* todo: add */

    public int GetAttachmentCount() => Audio.Count + Documents.Count + Pictures.Count + Video.Count;
}
