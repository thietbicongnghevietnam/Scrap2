using Microsoft.AspNetCore.Mvc;
using ScrapSystem.Web.Models;
using ScrapSystem.Web.Service.Interface;
using ScrapSystem.Web.Service;

namespace ScrapSystem.Web.Controllers
{
    public class MaterSectionController : Controller
    {
        private readonly IMaterSectionService _service;

        public MaterSectionController(IMaterSectionService service)
        {
            _service = service;
        }

        // ================= LIST VIEW =================
        public async Task<IActionResult> MaterSection(
            string keyword = "",
            int page = 1,
            int pageSize = 10,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var (data, total) = await _service.GetPagedAsync(keyword, page, pageSize, dateFrom, dateTo);

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.Keyword = keyword;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;

            return View("MaterSection",data);
        }

        // ================= AJAX LIST =================
        public async Task<IActionResult> MaterSectionList(
            string keyword = "",
            int page = 1,
            int pageSize = 10,
            string dateFrom = "",
            string dateTo = "")
        {
            DateTime? df = DateTime.TryParse(dateFrom, out var d1) ? d1 : null;
            DateTime? dt = DateTime.TryParse(dateTo, out var d2) ? d2 : null;

            var (data, total) = await _service.GetPagedAsync(keyword, page, pageSize, df, dt);

            return Json(new
            {
                data,
                total,
                page,
                pageSize
            });
        }

        // ================= DETAIL =================
        public async Task<IActionResult> MaterSectionDetail(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();

            return PartialView("_MaterSectionDetail", item);
        }

        // ================= CREATE =================
        public IActionResult CreateMaterSection()
        {
            return PartialView("_CreateEditMaterSection", new MaterSection());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMaterSection(MaterSection model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEditMaterSection", model);

            var result = await _service.CreateAsync(model);

            return Json(new { success = true, data = result });
        }

        // ================= EDIT =================
        public async Task<IActionResult> EditMaterSection(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();

            return PartialView("_CreateEditMaterSection", data);
        }

        [HttpPost]
        public async Task<IActionResult> EditMaterSection(MaterSection model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEditMaterSection", model);

            var result = await _service.UpdateAsync(model);

            return Json(new { success = true, data = result });
        }

        // ================= DELETE =================
        [HttpPost]
        public async Task<IActionResult> DeleteMaterSection(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return Json(new { success = ok, id });
        }


    }
}
