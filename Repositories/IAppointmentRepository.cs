using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<Appointment?> GetAppointmentWithDetailsAsync(int id);
        Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(string userId);
    }

}
