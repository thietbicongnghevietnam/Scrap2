using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
//using ExcelDataReader;
//Mở terminal hoặc Package Manager Console trong project ASP.NET Core và chạy:
//dotnet add package ExcelDataReader --version 3.7.1
//dotnet add package ExcelDataReader.DataSet --version 3.7.1

//cai dat export excel
//dotnet add package ClosedXML
using ClosedXML.Excel;


using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Api.Application.Service.IServices;
using ScrapSystem.Web.Data;
using ScrapSystem.Web.Models;
using System.Data;
using System.Drawing;


namespace ScrapSystem.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // ---------------- READ ----------------
        //public IActionResult Index()
        //{
        //    var list = _context.Users.AsNoTracking().ToList();
        //    //return View(list);
        //    return View("MaterUser/Users", list);
        //}
        public IActionResult Users()
        {
            var list = _context.Users.ToList();
            return View("Users", list);
        }

        // ---------------- CREATE ----------------
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateEdit", new Users());
        }

        [HttpPost]
        public IActionResult Create(Users model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEdit", model);
            }

            model.CreatedDate = DateTime.Now;
            model.Password = model.U_Pass; // copy password
            model.CreatedId = 1; // ví dụ lấy user login hiện tại

            _context.Users.Add(model);
            _context.SaveChanges();

            return Json(new { success = true, data = model });
        }

        // ---------------- EDIT ----------------
        public IActionResult Edit(int id)
        {
            var model = _context.Users.Find(id);
            if (model == null)
                return NotFound();

            // Không trả Password thật ra view
            model.U_Pass = model.Password;

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public IActionResult Edit(Users model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEdit", model);
            }

            var user = _context.Users.Find(model.Id);
            if (user == null)
                return NotFound();

            user.UserID = model.UserID;
            user.Section = model.Section;

            // Nếu có nhập password mới thì update
            if (!string.IsNullOrEmpty(model.U_Pass))
            {
                user.Password = model.U_Pass;
                user.U_Pass = model.U_Pass;
            }

            user.UpdatedDate = DateTime.Now;
            user.UpdatedId = 1; // user login hiện tại

            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ---------------- DELETE ----------------
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return Json(new { success = false });

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Json(new { success = true, id });
        }

        // ---------------- DETAILS ----------------
        [HttpGet]
        public IActionResult Details(int id)
        {
            var user = _context.Users
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == id);

            if (user == null)
                return NotFound();

            return PartialView("_Details", user);
        }

        [HttpGet]
        public IActionResult UsersList(
        string keyword = "",
        int page = 1,
        int pageSize = 5)
        {
            var query = _context.Users.AsNoTracking();

            // SEARCH
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    x.UserID.Contains(keyword) ||
                    x.Section.Contains(keyword));
            }

            int total = query.Count();

            var data = query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.UserID,
                    x.Section
                })
                .ToList();

            return Json(new { data, total });
        }

        //Mở terminal hoặc Package Manager Console trong project ASP.NET Core và chạy:
        //dotnet add package ExcelDataReader --version 3.7.1
        //dotnet add package ExcelDataReader.DataSet --version 3.7.1

        [HttpPost]
        public IActionResult UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select file" });

            var users = new List<Users>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = file.OpenReadStream())
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var table = result.Tables[0]; // Lấy sheet đầu tiên

                for (int i = 1; i < table.Rows.Count; i++) // Bỏ header row
                {
                    var row = table.Rows[i];
                    string userId = row[0]?.ToString().Trim();
                    if (string.IsNullOrEmpty(userId)) continue;

                    // Check trùng UserID
                    bool exists = _context.Users.Any(x => x.UserID == userId);
                    if (exists) continue;

                    var user = new Users
                    {
                        UserID = userId,
                        U_Pass = row[1]?.ToString(),
                        Password = row[1]?.ToString(),
                        Section = row[2]?.ToString(),
                        Department = row[3]?.ToString(),
                        CreatedDate = DateTime.Now,
                        CreatedId = 1
                    };
                    users.Add(user);
                }
            }

            if (users.Count > 0)
            {
                _context.Users.AddRange(users);
                _context.SaveChanges();
            }

            return Json(new
            {
                success = true,
                count = users.Count
            });
        }

        [HttpGet]
        public IActionResult ExportExcel(string keyword = "")
        {
            //var users = _userService.GetUsers(keyword); // lấy dữ liệu từ DB / SP
            // Gọi service lấy dữ liệu từ Stored Procedure
            var users = GetUsers(keyword);

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Users");

                // Header
                ws.Cell(1, 1).Value = "ID";
                ws.Cell(1, 2).Value = "UserID";
                ws.Cell(1, 3).Value = "Section";

                ws.Row(1).Style.Font.Bold = true;
                ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightBlue;

                // Data
                int row = 2;
                foreach (var u in users)
                {
                    ws.Cell(row, 1).Value = u.Id;
                    ws.Cell(row, 2).Value = u.UserID;
                    ws.Cell(row, 3).Value = u.Section;
                    row++;
                }

                ws.Columns().AdjustToContents(); // Autofit cột

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    string fileName = $"Users_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                fileName);
                }
            }
        }

        

        //public List<UserDto> GetUsers(string keyword)
        public List<Users> GetUsers(string keyword)
        {
            //var users = new List<UserDto>();
            var users = new List<Users>();

            var _connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_GetUsers_infor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Keyword", keyword ?? "");

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //users.Add(new UserDto
                        users.Add(new Users
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserID = reader["UserID"].ToString(),
                            Section = reader["Section"].ToString()
                        });
                    }
                }
            }

            return users;
        }



    }




}
