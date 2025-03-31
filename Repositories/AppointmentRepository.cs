using Microsoft.EntityFrameworkCore;
using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Appointment?> GetAppointmentWithDetailsAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.User) // Bao gồm thông tin User
                .Include(a => a.Service) // Bao gồm thông tin Service
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(string userId)
        {
            return await _context.Appointments
                .Include(a => a.Service) // Bao gồm thông tin Service
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }
    }
}
