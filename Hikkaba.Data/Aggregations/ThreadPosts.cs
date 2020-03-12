using System.Collections.Generic;
using Hikkaba.Data.Entities;

namespace Hikkaba.Data.Aggregations
{
    public class ThreadPosts
    {
        public Thread Thread { get; set; }
        public IList<Post> Posts { get; set; }
    }
}