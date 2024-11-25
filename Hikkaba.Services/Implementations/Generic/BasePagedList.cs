using System;
using System.Collections.Generic;
using Hikkaba.Models.Dto;

namespace Hikkaba.Services.Implementations.Generic;

public class BasePagedList<TViewModel>
{
    public int TotalPageCount
        => Convert.ToInt32(Math.Ceiling((double) TotalItemsCount/(double) CurrentPage.PageSize));
    public int TotalItemsCount { get; set; }
    public PageDto CurrentPage { get; set; }
    public IList<TViewModel> CurrentPageItems { get; set; }
}
