using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhom5_webAPI.Models;
using nhom5_webAPI.Repositories;
using System.Security.Claims;

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

        // Lấy tất cả Appointment (Chỉ cho phép Admin)
        [Authorize(Policy = "Appointment.View")]
        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            // Kiểm tra vai trò của người dùng
            if (User.IsInRole("Admin"))
            {
                // Admin có quyền xem tất cả các cuộc hẹn
                var appointments = await _appointmentRepository.GetAllAsync();
                return Ok(appointments);
            }

            // Nếu là User, chỉ có thể xem các cuộc hẹn của chính mình
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Lấy UserId từ Claims
            var userAppointments = await _appointmentRepository.GetAppointmentsByUserIdAsync(userId);

            // Nếu không có cuộc hẹn của user thì trả về NotFound
            if (!userAppointments.Any())
            {
                return NotFound(new { Error = "No appointments found for this user." });
            }

            // Trả về các cuộc hẹn của user
            return Ok(userAppointments);
        }


        // Lấy Appointment theo ID
        [HttpGet("{id}")]
        [Authorize(Policy = "Appointment.View")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);
            if (appointment == null)
                return NotFound(new { Error = "Appointment not found." });

            // Lấy UserName từ Claims của người dùng hiện tại
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                return Forbid(); // Trả về 403 nếu không tìm thấy UserName trong claims
            }

            // Lấy UserId từ UserName
            var user = await _userRepository.GetByUserNameAsync(userName);
            if (user == null)
            {
                return Forbid(); // Trả về 403 nếu không tìm thấy user
            }
            var currentUserId = user.Id;

            // Kiểm tra quyền truy cập
            if (!User.IsInRole("Admin") && appointment.UserId != currentUserId)
            {
                return Forbid(); // Trả về 403 nếu không phải Admin và không phải chủ sở hữu appointment
            }

            // Trả về thông tin appointment nếu hợp lệ
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
        [Authorize(Policy = "Appointment.View")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAppointmentsByUserId(string userId)
        {
            // Kiểm tra nếu User cố gắng xem cuộc hẹn của người khác
            if (User.IsInRole("User"))
            {
                if (userId == User.FindFirstValue(ClaimTypes.NameIdentifier)) // Nếu User cố gắng xem cuộc hẹn của người khác
                {
                    return Forbid(); // Người dùng không thể xem cuộc hẹn của người khác
                }
            }

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


        [Authorize(Policy = "Appointment.Create")]
        [HttpPost]
        public async Task<IActionResult> AddAppointment([FromBody] Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            
            var user = await _userRepository.GetByUserNameAsync(appointment.UserId); 
            if (user == null)
            {
                return BadRequest(new { Error = "Invalid UserName." });
            }

            var userId = user.Id; 

            if (appointment.UserId != user.UserName) 
            {
                return Forbid(); 
            }

            appointment.UserId = userId; 

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
