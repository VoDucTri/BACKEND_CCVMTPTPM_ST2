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

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments.ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(string userId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByUsernameAsync(string username)
        {
            // Tìm User dựa trên username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return new List<Appointment>(); // Trả về danh sách rỗng nếu không tìm thấy user
            }

            // Lấy các cuộc hẹn dựa trên UserId
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .Where(a => a.UserId == user.Id)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentWithDetailsAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
        }
    }
}
