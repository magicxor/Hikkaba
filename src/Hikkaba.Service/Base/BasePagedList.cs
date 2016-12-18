using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Dto;

namespace Hikkaba.Service.Base
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
