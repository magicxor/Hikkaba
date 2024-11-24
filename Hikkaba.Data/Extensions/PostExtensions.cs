using System.Collections.Generic;
using System.Linq;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;

namespace Hikkaba.Data.Extensions;

public static class PostExtensions
{
    public static IEnumerable<Audio> GetAudio(this Post post)
    {
        return post.Attachments.OfType<Audio>();
    }
        
    public static IEnumerable<Document> GetDocuments(this Post post)
    {
        return post.Attachments.OfType<Document>();
    }
        
    public static IEnumerable<Notice> GetNotices(this Post post)
    {
        return post.Attachments.OfType<Notice>();
    }
        
    public static IEnumerable<Picture> GetPictures(this Post post)
    {
        return post.Attachments.OfType<Picture>();
    }
        
    public static IEnumerable<Video> GetVideo(this Post post)
    {
        return post.Attachments.OfType<Video>();
    }
}