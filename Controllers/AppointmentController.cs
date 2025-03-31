using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhom5_webAPI.Models;
using nhom5_webAPI.Repositories;

namespace nhom5_webAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IServiceRepository _serviceRepository;

        // Constructor với tất cả dependency
        public AppointmentController(
            IAppointmentRepository appointmentRepository,
            IRepository<User> userRepository,
            IServiceRepository serviceRepository
        )
        {
            _appointmentRepository = appointmentRepository;
            _userRepository = userRepository;
            _serviceRepository = serviceRepository;
        }

        // Lấy tất cả Appointment
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            return Ok(appointments);
        }

        // Lấy Appointment theo ID
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);
            if (appointment == null)
                return NotFound(new { Error = "Appointment not found." });

            return Ok(new
            {
                appointment.AppointmentId,
                appointment.AppointmentDate,
                appointment.Status,
                User = new
                {
                    appointment.User.Id,
                    appointment.User.FullName,
                    appointment.User.Email
                },
                Service = new
                {
                    appointment.Service.ServiceId,
                    appointment.Service.Name,
                    appointment.Service.Price
                }
            });
        }

        // Lấy danh sách Appointment theo UserId
        [Authorize(Policy = "Appointment.View")]  // Policy-based Authorization
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAppointmentsByUserId(string userId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByUserIdAsync(userId);
            if (!appointments.Any())
                return NotFound(new { Error = "No appointments found for this user." });

            return Ok(appointments.Select(a => new
            {
                a.AppointmentId,
                a.AppointmentDate,
                a.Status,
                Service = new
                {
                    a.Service.ServiceId,
                    a.Service.Name,
                    a.Service.Price
                }
            }));
        }

        // Tạo mới Appointment
        [Authorize(Policy = "Appointment.Create")] // Policy-based Authorization
        [HttpPost]
        public async Task<IActionResult> AddAppointment([FromBody] Appointment appointment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra UserName và lấy UserId
            var user = await _userRepository.GetByUserNameAsync(appointment.UserId);
            if (user == null)
                return BadRequest(new { Error = "Invalid UserId." });

            appointment.UserId = user.Id; // Gán đúng UserId từ UserName

            try
            {
                await _appointmentRepository.AddAsync(appointment);
                await _appointmentRepository.SaveAsync();

                return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.AppointmentId }, appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while creating the appointment.", Details = ex.Message });
            }
        }

        // Cập nhật Appointment
        [Authorize(Policy = "Appointment.Edit")]  // Policy-based Authorization
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment appointment)
        {
            if (id != appointment.AppointmentId)
                return BadRequest(new { Error = "Appointment ID mismatch." });

            var existingAppointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);
            if (existingAppointment == null)
                return NotFound(new { Error = "Appointment not found." });

            // Cập nhật thông tin
            existingAppointment.AppointmentDate = appointment.AppointmentDate;
            existingAppointment.Status = appointment.Status;
            existingAppointment.ServiceId = appointment.ServiceId;

            try
            {
                _appointmentRepository.Update(existingAppointment);
                await _appointmentRepository.SaveAsync();

                return Ok(new { Message = "Appointment updated successfully.", Appointment = existingAppointment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while updating the appointment.", Details = ex.Message });
            }
        }

        // Xóa Appointment
        [Authorize(Policy = "Appointment.Delete")]  // Policy-based Authorization
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(new { Error = "Appointment not found." });

            try
            {
                _appointmentRepository.Delete(appointment);
                await _appointmentRepository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while deleting the appointment.", Details = ex.Message });
            }
        }
    }
}
