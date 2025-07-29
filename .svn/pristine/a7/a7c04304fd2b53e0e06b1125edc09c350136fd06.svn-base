using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapSystem.Api.Application.Request;
using ScrapSystem.Api.Application.Service;
using ScrapSystem.Api.Application.Service.IServices;

namespace ScrapSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VerifyController : Controller
    {
        private readonly IVerifyDataService _verifyDataService;
        public VerifyController(IVerifyDataService verifyDataService)
        {
            _verifyDataService = verifyDataService;
        }

        [HttpPost("data")]
        public async Task<IActionResult> VerifyData(VerifyRequest request)
        {
            try
            {
                var results = await _verifyDataService.VerifyDataAsync(request.File, request.Type);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
