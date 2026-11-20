namespace CmsApi.DTOs.UserDtos
{
    public class UserResponseDto : UserPaginationResponse
    {
        public List<UserItemDto> Items { get; set; } = [];
    }
}
