using CmsApi.Http.Handlers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BordersController : ControllerBase
    {
        private readonly ICmsHttpHandler _cmsHttpHandler;

        public BordersController(ICmsHttpHandler cmsHttpHandler)
        {
            _cmsHttpHandler = cmsHttpHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetBorders()
        {
            var result = await _cmsHttpHandler.GetBordersAsync();

            if (result is null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Borders API request failed");
            }

            return Ok(result);
        }
    }
}