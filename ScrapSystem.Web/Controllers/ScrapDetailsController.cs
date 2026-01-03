using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PMG_system.App_Code;
using ScrapSystem.Web.Data;
using ScrapSystem.Web.Models;
using System.Data;
using System.IO;
using System.Security.Claims;

namespace ScrapSystem.Web.Controllers
{
    public class ScrapDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public ScrapDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // READ (LIST)
        public async Task<IActionResult> Index()
        {
            //return View(await _context.ScrapDetails.ToListAsync());
            return View();
        }

        public IActionResult List(string keyword = "", int? sanctionId = null, string? section = null, string? typeName = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 5)
        {
            var query = _context.ScrapDetails
                .Include(x => x.Scrap)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                (x.Material ?? "").Contains(keyword) ||
                (x.CostCenter ?? "").Contains(keyword) ||
                (x.Plant ?? "").Contains(keyword) ||
                (x.Scrap.Sanction ?? "").Contains(keyword) ||
                (x.Scrap.Section ?? "").Contains(keyword));
            }

            if (sanctionId.HasValue)
            {
                query = query.Where(x => x.SanctionId == sanctionId.Value);
            }

            if (!string.IsNullOrEmpty(section))
            {
                query = query.Where(x => x.Scrap.Section == section);
            }

            if (!string.IsNullOrEmpty(typeName))
            {
                query = query.Where(x => x.TypeName == typeName);
            }

            // Filter From Date
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= fromDate.Value);
            }

            // Filter To Date (bao gồm hết ngày)
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedDate < endDate);
            }


            int total = query.Count();

            var data = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    Sanction = x.Scrap.Sanction,
                    Section = x.Scrap.Section,
                    x.Material,
                    x.Qty,
                    x.UnitPrice,
                    x.Amount,
                    x.CostCenter,
                    x.Plant,
                    x.TypeName,
                    x.MVT,
                    x.MoveType,
                    x.Sloc,
                    x.ScrapSloc,
                    CreatedDate = x.CreatedDate
                })
                .ToList();

            return Json(new { data, total });
        }

        public IActionResult GetSections()
        {
            var data = _context.Scraps
                .AsNoTracking()
                .Where(x => x.Section != null && x.Section != "")
                .Select(x => x.Section)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return Json(data);
        }

        public IActionResult GetSanctions()
        {
            var data = _context.Scraps
                .AsNoTracking()
                .OrderBy(x => x.Sanction)
                .Select(x => new
                {
                    x.Id,
                    x.Sanction
                })
                .ToList();

            return Json(data);
        }

        //loc section => sanction
        [HttpGet]
        public IActionResult GetSanctionsBySection(string section)
        {
            var sanctions = _context.Scraps
                .AsNoTracking()
                .Where(x => x.Section == section)
                .Select(x => new
                {
                    x.Id,
                    x.Sanction
                })
                .Distinct()
                .ToList();

            return Json(sanctions);
        }
        //load type name theo sacntion
        [HttpGet]
        public IActionResult GetTypeNameBySanction(int sanctionId)
        {
            var typeNames = _context.ScrapDetails
                .AsNoTracking()
                .Where(x => x.SanctionId == sanctionId && x.TypeName != null)
                .Select(x => x.TypeName)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return Json(typeNames);
        }

        // GET: Create (Modal)
        public IActionResult CreateModal()
        {
            ViewBag.Sanctions = _context.Scraps
            .AsNoTracking()
            .OrderBy(x => x.Sanction)
            .Select(x => new
            {
                x.Id,
                x.Sanction
            })
            .ToList();

            return PartialView("_Form", new ScrapDetail());
        }

        // GET: Edit (Modal)
        public async Task<IActionResult> EditModal(int id)
        {
            var model = await _context.ScrapDetails.FindAsync(id);
            if (model == null) return NotFound();

            ViewBag.Sanctions = _context.Scraps
                .AsNoTracking()
                .OrderBy(x => x.Sanction)
                .Select(x => new
                {
                    x.Id,
                    x.Sanction
                })
                .ToList();

            return PartialView("_Form", model);
        }

        // POST: Save (Create + Edit)
        [HttpPost]
        public async Task<IActionResult> Save(ScrapDetail model)
        {
            if (!ModelState.IsValid)
                return PartialView("_Form", model);

            if (model.Id == 0)
            {
                model.CreatedDate = DateTime.Now;
                _context.Add(model);
            }
            else
            {
                model.UpdatedDate = DateTime.Now;
                _context.Update(model);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var scrap = await _context.ScrapDetails.FindAsync(id);
            if (scrap == null) return NotFound();

            _context.ScrapDetails.Remove(scrap);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public IActionResult ExportIssueOut(string fromDate, string toDate, string sanction, string issueOut)
        {
            if (string.IsNullOrEmpty(sanction) || string.IsNullOrEmpty(issueOut))
            {
                return BadRequest("Sanction hoặc IssueOut không hợp lệ");
            }

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/Templates/Mau_IssueOutB.xlsx");

            string newFileName = $"IssueOut_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            string exportPath = Path.Combine(Path.GetTempPath(), newFileName);

            ProcessExcelFile(templatePath, exportPath, fromDate, toDate, sanction, issueOut);

            byte[] fileBytes = System.IO.File.ReadAllBytes(exportPath);
            System.IO.File.Delete(exportPath);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                newFileName
            );
        }

        private static void ProcessExcelFile(string filePath, string newFilePath, string tungay, string denngay, string sanction, string issueout)
        {
            try
            {
                FileInfo template = new FileInfo(filePath);
                FileInfo newFile = new FileInfo(newFilePath);

                if (!template.Exists)
                    throw new FileNotFoundException("Template Excel không tồn tại");


                DataTable dtAll = DataConn.StoreFillDS_new("Export_IssueOut_Data", CommandType.StoredProcedure,
                    new SqlParameter("@tungay", tungay),
                    new SqlParameter("@denngay", denngay),
                    new SqlParameter("@sanction", sanction),
                    new SqlParameter("@issueout", issueout)
                );
                DataTable dtTotal = DataConn.StoreFillDS_new("Export_IssueOut_total", CommandType.StoredProcedure,
                new SqlParameter("@tungay", tungay),
                new SqlParameter("@denngay", denngay),
                new SqlParameter("@sanction", sanction),
                new SqlParameter("@issueout", issueout)
                );
                DataTable dtAccount = DataConn.StoreFillDS_new("Export_IssueOut_MVT", CommandType.StoredProcedure,
                new SqlParameter("@tungay", tungay),
                new SqlParameter("@denngay", denngay),
                new SqlParameter("@sanction", sanction),
                new SqlParameter("@issueout", issueout)
                );

                if (dtAll.Rows.Count == 0)
                    return;

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(template))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets[1];

                    string[] parts = issueout.Split('.');

                    ws.Cells[2, 1].Value = parts.Length > 1 ? parts[1] : issueout;
                    ws.Cells[4, 4].Value = DateTime.Today;
                    ws.Cells[4, 4].Style.Numberformat.Format = "dd/MM/yyyy";

                    ws.Cells[6, 4].Value = dtAll.Rows[0]["MVT"].ToString();
                    ws.Cells[6, 6].Value = "Out";

                    ws.Cells[7, 4].Value = dtAccount.Rows[0][0];
                    ws.Cells[7, 6].Value = dtAccount.Rows[0][1];

                    ws.Cells[10, 4].Value = dtAll.Rows[0]["Vendor"];

                    int row = 16;
                    int i = 0;

                    foreach (DataRow dr in dtAll.Rows)
                    {
                        i++;
                        ws.Cells[row, 1].Value = i;
                        ws.Cells[row, 2].Value = dr["Plant"];
                        ws.Cells[row, 3].Value = dr["Sloc"];
                        ws.Cells[row, 4].Value = dr["CostCenter"];
                        ws.Cells[row, 5].Value = dr["NameCost"];
                        ws.Cells[row, 6].Value = dr["Material"];
                        ws.Cells[row, 7].Value = dr["Qty"];
                        ws.Cells[row, 8].Value = dr["UnitPrice"];
                        ws.Cells[row, 9].Value = dr["Amount"];
                        ws.Cells[row, 10].Value = dr["UnitPriceAC"];
                        ws.Cells[row, 11].Value = dr["AmountAC"];
                        ws.Cells[row, 12].Value = dr["Reason"];
                        row++;
                    }

                    ws.Cells[53, 7].Value = dtTotal.Rows[0][0];
                    ws.Cells[53, 9].Value = dtTotal.Rows[0][1];
                    ws.Cells[53, 11].Value = dtTotal.Rows[0][2];

                    package.SaveAs(newFile);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpGet]
        public IActionResult ExportScrapList(string fromDate, string toDate, string section, string sanctionId)
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate) || string.IsNullOrEmpty(section) || string.IsNullOrEmpty(sanctionId))
            {
                return BadRequest("Chưa chọn đủ thông tin");
            }

            string templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/Templates/mau sraplist.xlsx");

            string fileName = $"Export_Scraplist_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            string exportPath = Path.Combine(Path.GetTempPath(), fileName);

            ProcessExcelFileScrapList(templatePath, exportPath, fromDate, toDate, section, sanctionId);

            byte[] fileBytes = System.IO.File.ReadAllBytes(exportPath);
            System.IO.File.Delete(exportPath);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        private static void ProcessExcelFileScrapList(string filePath, string newFilePath, string tungay, string denngay, string bophan, string sanctionId)
        {
            FileInfo template = new FileInfo(filePath);
            FileInfo newFile = new FileInfo(newFilePath);

            if (!template.Exists)
                throw new FileNotFoundException("Không tìm thấy file mẫu Excel");

            DataTable dtExcel = DataConn.StoreFillDS_new(
                "Export_ScrapList_tool2_MVC",
                CommandType.StoredProcedure,
                new SqlParameter("@bophan", bophan),
                new SqlParameter("@sacnctionid", sanctionId),
                new SqlParameter("@tungay", tungay),
                new SqlParameter("@denngay", denngay)
            );

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //ExcelPackage.License = LicenseContext.NonCommercial;
            //set trong appsettings.json tu plus 8 tro len  (khong duoc)

            using (var package = new ExcelPackage(template))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets["Form B"];

                int row = 12;
                int i = 0;

                ws.Cells[8, 3].Value = DateTime.Today.ToString("MM");
                ws.Cells[9, 3].Value = bophan;

                foreach (DataRow dr in dtExcel.Rows)
                {
                    i++;
                    ws.Cells[row, 1].Value = i;
                    ws.Cells[row, 2].Value = dr["Plant"];
                    ws.Cells[row, 3].Value = dr["Sloc"];
                    ws.Cells[row, 4].Value = dr["CostCenter"];
                    ws.Cells[row, 5].Value = dr["NameCost"];
                    ws.Cells[row, 6].Value = dr["Material"];
                    ws.Cells[row, 7].Value = dr["Qty"];
                    ws.Cells[row, 8].Value = dr["UnitPrice"];
                    ws.Cells[row, 9].Value = dr["Amount"];

                    decimal unitPriceAC = dr["UnitPriceAC"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(dr["UnitPriceAC"]);

                    if (unitPriceAC == 0)
                    {
                        ws.Cells[row, 10].Value = null;
                        ws.Cells[row, 11].Value = null;
                    }
                    else
                    {
                        ws.Cells[row, 10].Value = dr["UnitPriceAC"];
                        ws.Cells[row, 11].Value = dr["AmountAC"];
                    }

                    ws.Cells[row, 12].Value = dr["Remark"];
                    ws.Cells[row, 13].Value = dr["VendorName"];
                    ws.Cells[row, 14].Value = dr["ScrapSloc"];
                    ws.Cells[row, 15].Value = "";
                    ws.Cells[row, 16].Value = dr["SanctionId"];
                    ws.Cells[row, 17].Value = dr["Reason"];
                    ws.Cells[row, 18].Value = bophan;
                    ws.Cells[row, 19].Value = dr["TypeName"];
                    ws.Cells[row, 20].Value = dr["MVT"];
                    ws.Cells[row, 21].Value = dr["MoveType"];
                    ws.Cells[row, 22].Value = dr["AccountCost"];
                    ws.Cells[row, 34].Value = dr["Vendor"];

                    row++;
                }

                // Xóa validation
                var validations = ws.DataValidations;
                for (int v = validations.Count - 1; v >= 0; v--)
                {
                    validations.Remove(validations[v]);
                }

                package.SaveAs(newFile);
            }
        }

        [HttpGet]
        public IActionResult Export_FA_PE(string fromDate,string toDate,string sanction,string issueOut)
        {
            if (string.IsNullOrEmpty(sanction) || sanction == "==Sanction==")
            {
                return BadRequest("NG, Du lieu Sanction or Issue out null!");
            }

            // đường dẫn file mẫu
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/Templates/Mau_DispositionProperty.xlsx");

            string fileName = $"DispositionProperty_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            string tempPath = Path.Combine(Path.GetTempPath(), fileName);

            ProcessExcelFile_FA_PE(templatePath,tempPath,fromDate,toDate,sanction,issueOut);

            byte[] fileBytes = System.IO.File.ReadAllBytes(tempPath);
            System.IO.File.Delete(tempPath);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );   
        }

        private static void ProcessExcelFile_FA_PE(string filePath,string newFilePath,string tungay,string denngay,string sanction,string issueout)
        {
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("File không tồn tại", filePath);

            //BẮT BUỘC: copy template → file output
            System.IO.File.Copy(filePath, newFilePath, true);

            DataTable dt_all = DataConn.StoreFillDS_new(
                "Export_Mau_FA_PE_MVC",
                CommandType.StoredProcedure,
                new SqlParameter("@tungay", tungay),
                new SqlParameter("@denngay", denngay),
                new SqlParameter("@sanction", sanction),
                new SqlParameter("@issueout", issueout)
            );

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(newFilePath)))
            {
                //ExcelWorksheet ws = package.Workbook.Worksheets[1];
                //if (ws == null)
                //    throw new Exception("Không tìm thấy worksheet");
                ExcelWorksheet ws = package.Workbook.Worksheets["Sheet1"];
                if (ws == null)
                    throw new Exception("Không tìm thấy sheet Sheet1");

                ws.Cells[4, 20].Value = DateTime.Today;

                int row = 6;
                int i = 0;

                foreach (DataRow dr in dt_all.Rows)
                {
                    i++;
                    ws.Cells[row, 1].Value = i;
                    ws.Cells[row, 2].Value = dr["ControlNo"];
                    ws.Cells[row, 3].Value = dr["Category"];
                    ws.Cells[row, 4].Value = "";
                    ws.Cells[row, 5].Value = "";
                    ws.Cells[row, 6].Value = "";
                    ws.Cells[row, 7].Value = dr["Qty"];
                    ws.Cells[row, 8].Value = "";
                    ws.Cells[row, 9].Value = dr["Material"];
                    ws.Cells[row, 10].Value = dr["Vendor"];
                    ws.Cells[row, 11].Value = dr["UnitPrice"];
                    ws.Cells[row, 12].Value = dr["Amount"];
                    ws.Cells[row, 13].Value = dr["BookValue"];
                    ws.Cells[row, 14].Value = dr["Currency"];
                    ws.Cells[row, 15].Value = dr["FaTool"];
                    ws.Cells[row, 16].Value = "";
                    ws.Cells[row, 17].Value = dr["SoTK"];
                    ws.Cells[row, 18].Value = dr["NgayTK"];
                    ws.Cells[row, 19].Value = "";
                    ws.Cells[row, 20].Value = dr["Reason"];
                    ws.Cells[row, 21].Value = dr["Pallet"];

                    row++;
                }

                package.Save(); 
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Confirm_Issue_Out(string fromDate,string toDate,string sanction,string issueOut,string bophan)
        {
            // 1Lấy UserID từ Session   //Dùng JWT để lấy userId
            //string userId = HttpContext.Session.GetString("UserId");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "NG, UserID not logged in !" });
            }

            userId = userId.Trim('"');

            // 2️ Validate input
            if (sanction == "==Sanction==" || bophan == "==Section==")
            {
                return Json(new { success = false, message = "NG, Du lieu Sanction or section null !" });
            }

            try
            {
                // 3️ Check user quyền Issue In/Out
                DataTable dt_checkuser = DataConn.StoreFillDS_new(
                "CheckUser_Issue_InOut",
                CommandType.StoredProcedure,
                new SqlParameter("@userId", userId),
                new SqlParameter("@bophan", bophan),
                new SqlParameter("@sanction", sanction)
                
            );

                if (dt_checkuser.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "NG, Không xác định quyền user" });
                }

                string result = dt_checkuser.Rows[0][0].ToString();

                // ===== CASE 1: OK – được tạo Issue Out =====
                if (result == "1")
                {
                    bool ok = CreateIssueOut(
                        fromDate,
                        toDate,
                        sanction,
                        issueOut,
                        bophan,
                        userId,
                        out string errorMsg
                    );

                    if (!ok)
                    {
                        return Json(new { success = false, message = errorMsg });
                    }

                    // update trạng thái
                    DataTable dt_u = DataConn.StoreFillDS_new(
                "Update_ScrapList_Isssue_In_Out",
                CommandType.StoredProcedure,
                new SqlParameter("@bophan", bophan),
                new SqlParameter("@sanction", sanction),
                new SqlParameter("@fromDate", fromDate),
                new SqlParameter("@toDate", toDate)
                );

                    return Json(new 
                    { 
                        success = true, 
                        message = "Create Issue Out successful!" 
                    });
                }

                // ===== CASE 2: Đã tạo E-Pro → hỏi confirm reset =====
                if (result == "2")
                {
                    return Json(new
                    {
                        success = false,
                        needConfirm = true,
                        message = "Sanction này đã được tạo E-Pro. Bạn có muốn Reset lại Issue Out không?"
                    });
                }

                // ===== CASE 3: Không có quyền =====
                return Json(new
                {
                    success = false,
                    message = "NG, User khong co quyen tao In Out, Kiem tra lai!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool CreateIssueOut(string fromDate,string toDate,string sanction,string issueOut,string bophan,string userId,out string error)
        {
            error = "";

            // === toàn bộ code nghiệp vụ của bạn ===
            // - Get_VMT_Issue_InOut
            // - Select_Issue_InOut
            // - Get_Max_Issue_InOut
            // - Build SQL insert
            // - Execute_NonSQL3
            // 👉 GIỮ NGUYÊN 100% LOGIC

            try
            {
                // ví dụ:
                DataTable dt_typeMVT = DataConn.StoreFillDS_new(
                    "Get_VMT_Issue_InOut",
                    CommandType.StoredProcedure,
                    new SqlParameter("@sanction", sanction),
                    new SqlParameter("@bophan", bophan)
                );

                if (dt_typeMVT.Rows.Count == 0)
                {
                    error = "MVT is null, check again";
                    return false;
                }

                // 👉 copy phần for + SQL insert từ WebForm sang đây

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }






    }
}
