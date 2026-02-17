using Microsoft.EntityFrameworkCore;
using ScrapSystem.Web.Data;
using ScrapSystem.Web.Models;
using ScrapSystem.Web.Service.Interface;

namespace ScrapSystem.Web.Service
{
    //public class MaterSectionService
    //{
    //}
    public class MaterSectionService : IMaterSectionService
    {
        private readonly AppDbContext _context;

        public MaterSectionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<MaterSection> data, int total)> GetPagedAsync(string keyword,int page,int pageSize,DateTime? dateFrom,DateTime? dateTo)
        {
            var query = _context.MaterSections.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Section.Contains(keyword));

            if (dateFrom.HasValue)
                query = query.Where(x => x.Createdate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(x => x.Createdate <= dateTo.Value.AddDays(1));

            int total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<MaterSection> GetByIdAsync(int id)
        {
            return await _context.MaterSections.FindAsync(id);
        }

        public async Task<MaterSection> CreateAsync(MaterSection model)
        {
            _context.MaterSections.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MaterSection> UpdateAsync(MaterSection model)
        {
            _context.MaterSections.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.MaterSections.FindAsync(id);
            if (entity == null) return false;

            _context.MaterSections.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
