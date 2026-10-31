namespace CmsApi.DTOs.UserDtos
{
    public class UserRequestDto
    {
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;

        // Ad, soyad, ata adı, username və FIN üzrə axtarış
        public string? Search { get; set; }

        // NULL olduqda bütün rollar
        public int? RoleId { get; set; }

        // NULL olduqda bütün ərazi idarələri
        public string? RegionalOfficeId { get; set; }

        // NULL olduqda bütün statuslar
        public bool? IsActive { get; set; }
    }
}
