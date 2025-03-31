using nhom5_webAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nhom5_webAPI.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductsWithDetailsAsync();
        Task<Product?> GetProductWithDetailsByIdAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<PetSupplyCategory?> GetSupplyCategoryByIdAsync(int supplyCategoryId);
    }
}
