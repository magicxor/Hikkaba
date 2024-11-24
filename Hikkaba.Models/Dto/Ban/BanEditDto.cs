using System;
using Hikkaba.Models.Dto.Base.Current;

namespace Hikkaba.Models.Dto;

public class BanEditDto: IBaseDto
{
    public TPrimaryKey Id { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string LowerIpAddress { get; set; }
    public string UpperIpAddress { get; set; }
    public string Reason { get; set; }
    public PostDto RelatedPost { get; set; }
    public CategoryDto Category { get; set; }
}
