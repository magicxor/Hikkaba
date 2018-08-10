using System;
using System.Collections.Generic;
using Hikkaba.Models.Dto;

namespace Hikkaba.Services.Base.Generic
{
    public class BasePagedList<TDto>
    {
        public int TotalPageCount
            => Convert.ToInt32(Math.Ceiling((double) TotalItemsCount/(double) CurrentPage.PageSize));
        public int TotalItemsCount { get; set; }
        public PageDto CurrentPage { get; set; }
        public IList<TDto> CurrentPageItems { get; set; }
    }
}
