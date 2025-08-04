using Azure.Core;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PMG_system.App_Code;
using ScrapSystem.Api.Application.DTOs.LabelListDtos;
using ScrapSystem.Api.Application.DTOs.ScrapDetailDtos;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Application.DTOs.ScrapImageDtos;
using ScrapSystem.Api.Application.DTOs.VerifyDataDtos;
using ScrapSystem.Api.Application.Request;
using ScrapSystem.Api.Application.Response;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Repositories;
using ScrapSystem.Api.Utilities;
using ScrapSystem.Web.Dtos;
using ScrapSystem.Web.Models;
using ScrapSystem.Web.Service.Interface;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using X.PagedList;

namespace ScrapSystem.Web.Controllers
{
    [AuthorizeApi]
    public class ScrapController : BaseController
    {
        private readonly IApiClientService _apiClientService;
        private readonly AppDbContext _context;
        public ScrapController(IApiClientService apiClientService, AppDbContext context) //, AppDbContext context
        {
            _apiClientService = apiClientService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Verify()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime startDate, DateTime endDate, string sanction = "")
        {
            try
            {
                startDate = startDate == default ? DateTime.Now : startDate;
                endDate = endDate == default ? DateTime.Now : endDate;
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("startDate", startDate.ToString("yyyy-MM-dd"));
                data.Add("endDate", endDate.ToString("yyyy-MM-dd"));
                if (!string.IsNullOrEmpty(sanction))
                    data.Add("sanction", sanction);
                var res = await _apiClientService.GetAsync<ApiResult<MasterDetailDto<LabelListMasterDto, LabelListDetailDto>>>("api/Scrap/label-list", data);
                if (!res.IsSuccess)
                    return View();
                ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
                ViewBag.Sanction = sanction;
                return View(res.MasterDetail);
            }
            catch (Exception)
            {
                return View();
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Verify(VerifyRequest request)
        {
            Dictionary<string, IFormFile> files = new Dictionary<string, IFormFile>();
            Dictionary<string, string> param = new Dictionary<string, string>();
            files.Add("File", request.File);
            param.Add("Type", request.Type);
            var res = await _apiClientService.PostFileAsync("api/Verify/data", files, param);
            var rs = JsonConvert.DeserializeObject<List<List<VerificationResult>>>(res.MasterDetail.ToString());
            return View(rs);
        }

        //[WebMethod]
        //public static string Clear_scanbox(string serial_outer, string serial_inner, string cate_name, string modelname)
        //{
        //    String thongbao = "";
        //    DataTable dt4 = new DataTable();
        //    //check xem user co duoc phan quyen cap 2 khong?
        //    dt4 = DataConn.StoreFillDS("pro_get_serialinner_clearstatus", System.Data.CommandType.StoredProcedure, serial_outer, serial_inner, cate_name, modelname);
        //    if (dt4.Rows[0][0].ToString() == "1")
        //    {
        //        thongbao = "OK";// + "," + dt4.Rows[0][1].ToString() + "," + dt4.Rows[0][2].ToString();
        //    }
        //    else
        //    {
        //        thongbao = "NG";
        //    }

        //    return thongbao;
        //}
        
        [HttpGet]
        public async Task<IActionResult> Pheduyet([FromQuery] ScrapRequest request)
        {         
            string thongbao = "";
            DataTable dt4 = new DataTable();
            //check xem user co duoc phan quyen cap 2 khong?            
            request.Sanction = request.Sanction == null ? "" : request.Sanction;
            string tensanction = request.Sanction;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tensanction == "")
            {
                thongbao = "Sanction is null";
                return Ok(thongbao);
                //return Json(new { script = "alert('Vui lòng chọn Sanction trước khi phê duyệt.');" });
            }
            else 
            {
                dt4 = DataConn.StoreFillDS("pheduyetsanction", System.Data.CommandType.StoredProcedure, tensanction, userId);
                if (dt4.Rows[0][0].ToString() == "1")
                {
                    thongbao = "OK";// + "," + dt4.Rows[0][1].ToString() + "," + dt4.Rows[0][2].ToString();
                }
                else if (dt4.Rows[0][0].ToString() == "2") 
                {
                    thongbao = "Hang chua kiem tra het";
                }
                else
                {
                    thongbao = "NG";
                }
                //return RedirectToAction("Report", "Scrap");
                return Ok(thongbao);
            }            
            
        }


        [HttpGet]
        public async Task<IActionResult> Report([FromQuery] ScrapRequest request)
        {
            request.StartDate = request.StartDate == default ? DateTime.Now.AddMonths(-5) : request.StartDate;
            request.EndDate = request.EndDate == default ? DateTime.Now : request.EndDate;
            request.Page = request.Page <= 0 ? 1 : request.Page;
            request.PageSize = request.PageSize <= 0 ? 25 : request.PageSize;
            request.Sanction = request.Sanction == null? "" : request.Sanction;
            var rs = await _apiClientService.PostAsync<ApiResult<ScrapViewDto>>("api/Scrap/load-data", request);

            if (rs == null || rs.PagedResult?.Records == null)
            {
                return View(new StaticPagedList<ScrapViewDto>(new List<ScrapViewDto>(), request.Page, request.PageSize, 0));
            }

            ViewBag.StartDate = request.StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = request.EndDate.ToString("yyyy-MM-dd");
            ViewBag.Status = request.Status;
            ViewBag.Sanction = request.Sanction;
            ViewBag.PageSize = request.PageSize;

            var result = new ReportViewModel<ScrapViewDto>
            {
                SelectedStatus = request.Status,
                PageLists = new StaticPagedList<ScrapViewDto>(rs.PagedResult.Records, request.Page, request.PageSize, rs.PagedResult.TotalCount)

            };

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Import(ImportRequest request)
        {
            Dictionary<string, IFormFile> files = new Dictionary<string, IFormFile>();
            Dictionary<string, string> param = new Dictionary<string, string>();

            //kiem tra xem Sanction => co hang len pallet chua?
            //neu da kiem tra roi thi khong duoc upload
            DataTable dt4 = new DataTable();
            dt4 = DataConn.StoreFillDS("CheckPalletID_upload", System.Data.CommandType.StoredProcedure, request.Sanction, request.Section, request.issueout);
            if (dt4.Rows[0][0].ToString() == "0")
            {
                //truong hop hang da len pallet (da check roi) roi khong duoc upload nua
                return Ok();
            }
            else 
            {
                files.Add("file", request.File);
                param.Add("sanction", request.Sanction);
                param.Add("section", request.Section);
                param.Add("issueout", request.issueout);
                var res = await _apiClientService.PostFileAsync("api/Scrap/import", files, param);
                var rs = JsonConvert.DeserializeObject<ParentWithChildren<ScrapDto, ScrapDetailDto>>(res.MasterDetail.ToString());                
                return Ok(rs);
            }                        
            
        }

        [HttpPost]
        public async Task<IActionResult> ImportFileName(ImportRequest request)
        {
            var rs = await _apiClientService.PostAsync<ApiResult<ScrapViewDto>>("api/Scrap/import-material-name", request);
            return Ok(rs.Items);
        }

        [HttpGet]
        public async Task<IActionResult> LoadImage(string sanctionId, string pallet)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("sanctionId", sanctionId);
            data.Add("pallet", pallet);
            var rs = await _apiClientService.GetAsync<ApiResult<MasterDetailDto<ScrapImageDto, ScrapImageDetailDto>>>("api/Scrap/load-image", data);
            return Ok(rs.MasterDetail);
        }

        [HttpGet]
        public async Task<IActionResult> LoadImage2(string sanctionId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Sanction", sanctionId);
            parameters.Add("@pallet", 1);
            var _connectionString = _context.Database.GetDbConnection().ConnectionString;
            //var _connectionString = @"Server=10.92.186.30;Database=ScrapSystem;User Id=sa;Password=Psnvdb2013;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var multi = await connection.QueryMultipleAsync(
                        "GetImageScrap2",
                        parameters,
                        commandType: CommandType.StoredProcedure))
                    {
                        var masters = (await multi.ReadAsync<ScrapImageDto2>()).ToList();
                        var details = (await multi.ReadAsync<ScrapImageDetailDto2>()).ToList();

                        var rs = new ApiResult<MasterDetailDto2<ScrapImageDto2, ScrapImageDetailDto2>>
                        {
                            IsSuccess = true,
                            MasterDetail = new MasterDetailDto2<ScrapImageDto2, ScrapImageDetailDto2>
                            {
                                Masters2 = masters,
                                Details2 = details
                            }
                        };

                        return Ok(rs.MasterDetail);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GenerateAppendix(DateTime startDate, DateTime endDate, string appendix)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("startDate", startDate.ToString("yyyy-MM-dd"));
            param.Add("endDate", endDate.ToString("yyyy-MM-dd"));
            param.Add("appendix", appendix);
            var rs = await _apiClientService.GetAsync<ApiResult<byte[]>>("api/Scrap/generate-appendix", param);

            var fileBytes = rs.Item;
            var fileName = $"Appendix_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ExportToPdf(List<string> beforeImages, List<string> afterImages, string barcode, DateTime date)
        {
            //update trang thai export cho user vao bang image
            DataTable dt4 = new DataTable();
            string tensanction = barcode.Split(';')[0].ToString();
            string palletno = barcode.Split(';')[1].ToString();
            string bophan = barcode.Split(';')[2].ToString();
            dt4 = DataConn.StoreFillDS("UpdateExportFlag", System.Data.CommandType.StoredProcedure, tensanction, palletno, bophan);
            if (dt4.Rows[0][0].ToString() == "1")
            {

            }
            var rs = PdfHelper.ExportImagesToPdf(beforeImages, afterImages, barcode, date);
            return File(rs,
                        "application/pdf",
                        $"ExportedPdf_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> showToPdf(List<string> beforeImages, List<string> afterImages, string barcode, DateTime date)
        {

            var rs = PdfHelper.ShowImagesToPdf(beforeImages, afterImages, barcode, date);
            return File(rs, "application/pdf", enableRangeProcessing: true);

            //Response.Headers.Add("Content-Disposition", "inline; filename=Exported.pdf");
            //return File(rs, "application/pdf");
            //return File(rs,
            //            "application/pdf",
            //            $"ExportedPdf_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        [HttpDelete("DeleteScrapDetail/{id}")]
        public async Task<IActionResult> DeleteScrapDetail(int id)
        {
            try
            {
                await _apiClientService.DeleteAsync($"api/Scrap/scrap-detail/{id}");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
            return Ok();
        }

        [HttpPut("UpdateScrapDetail/{id}")]
        public async Task<IActionResult> UpdateScrapDetail(int id, int qty)
        {
            try
            {
                var data = new UpdateQtyRequest { Qty = qty };

                var res = await _apiClientService.PutAsync<ApiResult<bool>>($"api/Scrap/scrap-detail/{id}", data);
                if (!res.IsSuccess)
                    return BadRequest();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}
