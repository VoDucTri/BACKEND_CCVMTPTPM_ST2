using Microsoft.EntityFrameworkCore;
using nhom5_webAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nhom5_webAPI.Repositories
{
    public class PetRepository : Repository<Pet>, IPetRepository
    {
        private readonly ApplicationDbContext _context;

        public PetRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pet>> GetAllAsync()
        {
            return await _context.Pets
                .Include(p => p.Images) // Tải hình ảnh liên quan
                .ToListAsync();
        }

        public async Task<Pet> GetByIdAsync(int id)
        {
            return await _context.Pets
                .Include(p => p.Images) // Tải hình ảnh liên quan
                .FirstOrDefaultAsync(p => p.PetId == id);
        }

        public async Task<IEnumerable<Pet>> GetPetsByCategoryAsync(int categoryId)
        {
            return await _context.Pets
                .Where(p => p.CategoryId == categoryId) // Lọc theo CategoryId
                .Include(p => p.Images) // Tải hình ảnh liên quan
                .ToListAsync();
        }

        public async Task<PetCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.PetCategories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }
    }

}
