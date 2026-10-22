namespace CmsApi.DTOs.UserDtos
{
    public class UserPaginationResponse
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrev { get; set; }
        public bool HasNext { get; set; }
    }
}
