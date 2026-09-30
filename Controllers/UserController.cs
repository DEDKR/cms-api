using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.UserDtos;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> GetUsers([FromBody] UserRequestDto request)
        {
            var users = await _userRepository.GetUsers(request.PageSize, request.PageNumber);
            return Ok(ApiResponse<UserResponseDto>.Ok(users));
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetUserRoles()
        {
            var roles = await _userRepository.GetUserRoles();
            return Ok(ApiResponse<List<UserRolesResponse>>.Ok(roles));
        }
    }
}

