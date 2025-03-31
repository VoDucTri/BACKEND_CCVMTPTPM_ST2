using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public interface IPetRepository : IRepository<Pet>
    {
        Task<IEnumerable<Pet>> GetAllAsync();
        Task<Pet> GetByIdAsync(int id);
        Task<IEnumerable<Pet>> GetPetsByCategoryAsync(int categoryId);
        Task<PetCategory?> GetCategoryByIdAsync(int categoryId);
    }


}

