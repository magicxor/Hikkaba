namespace Hikkaba.Common.Dto
{
    public class PageDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public PageDto()
        {
            PageNumber = 1;
            PageSize = 10;
        }

        public PageDto(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public int Skip => (PageNumber - 1) * PageSize;
    }
}
