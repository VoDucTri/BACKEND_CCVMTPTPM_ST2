using Microsoft.EntityFrameworkCore;
using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public class ServiceRepository : Repository<Service>, IServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllServicesWithImagesAsync()
        {
            return await _context.Services.Include(s => s.Images).ToListAsync();
        }

        public async Task<Service> GetServiceByIdWithImagesAsync(int id)
        {
            return await _context.Services.Include(s => s.Images).FirstOrDefaultAsync(s => s.ServiceId == id);
        }
        public async Task AddAsync(Service entity)
        {
            await _context.Services.AddAsync(entity); // Thêm dịch vụ
            await _context.SaveChangesAsync(); // Lưu thay đổi
        }
        public async Task UpdateAsync(Service entity)
        {
            _context.Services.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Service entity)
        {
            _context.Services.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

}
