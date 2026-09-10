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

        [HttpGet("{z:int}/{x:int}/{y:int}")]
        public async Task<IActionResult> GetBorders(int z, int x, int y)
        {
            var result = await _cmsHttpHandler.GetBorderTileAsync(z, x, y);

            if (result is null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Borders API request failed");
            }


            //return Ok(result);

            return File(
            result,
            "application/x-protobuf");
        }
    }
}