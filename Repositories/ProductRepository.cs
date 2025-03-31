using Microsoft.EntityFrameworkCore;
using nhom5_webAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nhom5_webAPI.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsWithDetailsAsync()
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.SupplyCategory)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithDetailsByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.SupplyCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int SupplyCategoryId)
        {
            return await _context.Products
                .Where(p => p.SupplyCategoryId == SupplyCategoryId)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<PetSupplyCategory?> GetSupplyCategoryByIdAsync(int supplyCategoryId)
        {
            return await _context.PetSupplyCategories.FirstOrDefaultAsync(sc => sc.SupplyCategoryId == supplyCategoryId);
        }
    }
}
