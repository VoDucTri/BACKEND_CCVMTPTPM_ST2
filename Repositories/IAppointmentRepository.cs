using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(string userId);
        Task<IEnumerable<Appointment>> GetAppointmentsByUsernameAsync(string username);
        Task<Appointment?> GetAppointmentWithDetailsAsync(int id);
    }

}
