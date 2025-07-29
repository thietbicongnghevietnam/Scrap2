using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ScrapSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProtectedController : ControllerBase
    {
        [HttpGet("name")]
        [Authorize(Roles = "Admin")]
        public IActionResult Get()
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
                return Ok(user);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        [HttpGet("role")]
        [Authorize(Roles = "User")]
        public IActionResult GetRole()
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
                return Ok(user);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        [HttpGet("pdf")]
        public IActionResult Pdf()
        {
            try
            {
                //PdfHelper.ExportImagesByFolders(@$"E:\folders_2_columns{DateTime.Now.ToString("hhmmss")}.pdf",
                //                             @"E:\Images\A\IAF17520",
                //                             @"E:\Images\B\IAF17520",
                //                             "After", "Before");
                return Ok();

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }
    }
}
