using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hikkaba.Web.ViewModels.PostsViewModels;

namespace Hikkaba.Web.ViewModels.ThreadsViewModels;

public sealed class ThreadDetailsViewModel
{
    public required long Id { get; set; }

    [Display(Name = @"Is deleted")]
    public required bool IsDeleted { get; set; }

    [Display(Name = @"Created at")]
    public required DateTime CreatedAt { get; set; }

    [Display(Name = @"Modified at")]
    public required DateTime? ModifiedAt { get; set; }

    [Display(Name = @"Title")]
    public required string Title { get; set; }

    [Display(Name = @"Is pinned")]
    public required bool IsPinned { get; set; }

    [Display(Name = @"Is closed")]
    public required bool IsClosed { get; set; }

    [Display(Name = @"Is cyclic")]
    public required bool IsCyclic { get; set; }

    [Display(Name = @"Bump limit")]
    public required int BumpLimit { get; set; }

    [Display(Name = @"Show thread-local user hashes")]
    public required bool ShowThreadLocalUserHash { get; set; }

    public required int CategoryId { get; set; }

    [Display(Name = @"Category alias")]
    public required string CategoryAlias { get; set; }

    [Display(Name = @"Category name")]
    public required string CategoryName { get; set; }

    [Display(Name = @"Post count")]
    public required int PostCount { get; set; }

    public required IReadOnlyList<PostDetailsViewModel> Posts { get; set; }
}
