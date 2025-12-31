using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Web.Data;
using ScrapSystem.Web.Models;

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

        public IActionResult List(string keyword = "", int? sanctionId = null, string? section = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 5)
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


    }
}
