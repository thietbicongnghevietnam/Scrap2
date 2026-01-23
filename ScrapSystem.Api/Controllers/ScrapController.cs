using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ScrapSystem.Api.Application.Request;
using ScrapSystem.Api.Application.Service.IServices;
using System.Data;

namespace ScrapSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScrapController : ControllerBase
    {
        private readonly IImportScrapService _importScrapService;
        private readonly IVerifyDataService _verifyDataService;

        public ScrapController(IImportScrapService importScrapService, IVerifyDataService verifyDataService)
        {
            _importScrapService = importScrapService;
            _verifyDataService = verifyDataService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFile(ImportRequest request)
        {
            var rs = await _importScrapService.ImportScrapAsync(request.File, request.Sanction, request.Section, request.issueout, request.IsMergeSanction);
            //var rs = await _importScrapService.ImportScrapAsync(request.File, request.Sanction, request.Section, request.issueout, request.SelectedSection);

            if (!rs.IsSuccess)
            {
                return BadRequest(rs);
            }

            return Ok(rs);
        }
         
        [HttpDelete("scrap-detail/{id}")]
        public async Task<IActionResult> DeleteScrapDetail(int id)
        {
            var rs = await _importScrapService.DeleleScrapDetailById(id);

            if (!rs.IsSuccess)
            {
                return BadRequest(rs);
            }

            return Ok(rs);
        }

        [HttpPut("scrap-detail/{id}")]
        public async Task<IActionResult> UpdateScrapDetail( int id, [FromBody] UpdateQtyRequest request)
        {
            var rs = await _importScrapService.UpdateQtyScrapDetail(id, request.Qty, request.QtyActual);

            if (!rs.IsSuccess)
            {
                return BadRequest(rs);
            }

            return Ok(rs);
        }

        [HttpPost("import-material-name")]
        public async Task<IActionResult> ImportMaterialName(IFormFile file)
        {
            var rs = await _importScrapService.ImportMaterialNameAsync(file);

            if (!rs.IsSuccess)
            {
                return BadRequest(rs);
            }

            return Ok(rs);
        }

        [HttpPost("load-data")]
        //public async Task<IActionResult> LoadData(ScrapRequest request)
        public async Task<IActionResult> LoadData(ScrapRequest2 request)
        {
            var rs = await _importScrapService.LoadData(request);

            if (!rs.IsSuccess)
            {
                return BadRequest(rs);
            }

            return Ok(rs);
        }

        [HttpGet("load-image")]
        public async Task<IActionResult> LoadImage(string sanctionId, string? pallet)
        {
            var rs = await _importScrapService.LoadImage(sanctionId, pallet);

            if (!rs.IsSuccess)
            {
                return BadRequest(rs);
            }

            return Ok(rs);
        }

        [HttpGet("generate-appendix")]
        public async Task<IActionResult> GenerateAppendix(DateTime startDate, DateTime endDate, string appendix)
        {
            var rs = await _verifyDataService.GenarateAppendix(startDate, endDate, Convert.ToInt32(appendix));

            if (!rs.IsSuccess)
            {
                return BadRequest(rs);
            }

            return Ok(rs);
        }

        [HttpGet("label-list")]
        public async Task<IActionResult> GetLabelList([FromQuery]DateTime startDate, DateTime endDate, string sanction = "", string section="")
        {
            var rs = await _importScrapService.GetLabelListAsync(startDate, endDate, sanction, section);

            if (!rs.IsSuccess)
            {
                return BadRequest(rs);
            }

            return Ok(rs);
        }

        [HttpPost]
        [Route("GetNameSection")]
        [AllowAnonymous]       //khong chay authen
        public async Task<IActionResult> GetNameSection([FromBody] Dictionary<string, string> requestData)
        {
            try
            {
                if (!requestData.ContainsKey("SanctionID"))
                {
                    return BadRequest("Missing DATA in request data.");
                }
                // Gọi phương thức để lấy dữ liệu từ cơ sở dữ liệu
                DataTable table = await Task.FromResult<DataTable>(
                    DbconnectScrap.StoreFillDS(nameof(GetNameSection), CommandType.StoredProcedure, requestData["SanctionID"])
                );

                // Chuyển DataTable thành JSON
                string json = DataTableToJson(table);

                // Trả về kết quả JSON
                return Ok(json);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về mã lỗi 500 cùng thông điệp
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private string DataTableToJson(DataTable table)
        {
            var jsonResult = JsonConvert.SerializeObject(table);
            return jsonResult;
        }

    }
}
