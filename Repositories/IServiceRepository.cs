using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public interface IServiceRepository : IRepository<Service>
    {
        Task<IEnumerable<Service>> GetAllServicesWithImagesAsync();
        Task<Service> GetServiceByIdWithImagesAsync(int id);
        Task UpdateAsync(Service entity); // Thêm phương thức UpdateAsync
        Task DeleteAsync(Service entity); // Thêm phương thức DeleteAsync

    }

}
