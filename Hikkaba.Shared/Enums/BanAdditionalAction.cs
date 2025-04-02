using System.ComponentModel.DataAnnotations;

namespace Hikkaba.Shared.Enums;

public enum BanAdditionalAction
{
    [Display(Name = "None")]
    None,

    [Display(Name = "Delete this post")]
    DeletePost,

    [Display(Name = "Delete all posts from banned IP(s) in this thread")]
    DeleteAllPostsInThread,

    [Display(Name = "Delete all posts from banned IP(s) in this category")]
    DeleteAllPostsInCategory,

    [Display(Name = "Delete all posts from banned IP(s) in all categories")]
    DeleteAllPosts,
}
