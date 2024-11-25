using System;
using Hikkaba.Models.Dto.Base.Current;
using Hikkaba.Models.Dto.Post;

namespace Hikkaba.Models.Dto.Ban;

public class BanDto: BaseMutableDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string LowerIpAddress { get; set; }
    public string UpperIpAddress { get; set; }
    public string Reason { get; set; }
    public PostDto RelatedPost { get; set; }
    public CategoryDto Category { get; set; }
}