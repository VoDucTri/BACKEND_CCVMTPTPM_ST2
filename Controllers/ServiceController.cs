using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhom5_webAPI.Models;
using nhom5_webAPI.Repositories;

namespace nhom5_webAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceController(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        // Allow public access to view services
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _serviceRepository.GetAllServicesWithImagesAsync();
            return Ok(services);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var service = await _serviceRepository.GetServiceByIdWithImagesAsync(id);
            if (service == null) return NotFound("Service not found.");
            return Ok(service);
        }

        // Thêm Service - chỉ cho phép Admin
        [HttpPost]
        [Authorize(Policy = "Service.Create")] // Policy-based Authorization
        public async Task<IActionResult> AddService([FromBody] Service service)
        {
            if (service == null)
                return BadRequest("Invalid service data.");

            if (service.Images != null)
            {
                foreach (var image in service.Images)
                {
                    image.Service = null;
                }
            }

            await _serviceRepository.AddAsync(service);
            return Ok(new { message = "Service added successfully.", service });
        }

        // Cập nhật Service - chỉ cho phép Admin
        [HttpPut("{id}")]
        [Authorize(Policy = "Service.Edit")] // Policy-based Authorization
        public async Task<IActionResult> UpdateService(int id, [FromBody] Service updatedService)
        {
            if (updatedService == null)
                return BadRequest("The updatedService field is required.");

            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null)
                return NotFound("Service not found.");

            service.Name = updatedService.Name;
            service.Description = updatedService.Description;
            service.Price = updatedService.Price;
            service.Images = updatedService.Images;

            await _serviceRepository.UpdateAsync(service);
            return Ok(new { message = "Service updated successfully.", service });
        }

        // Xóa Service - chỉ cho phép Admin
        [HttpDelete("{id}")]
        [Authorize(Policy = "Service.Delete")] // Policy-based Authorization
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null) return NotFound("Service not found.");

            await _serviceRepository.DeleteAsync(service);
            return Ok("Service deleted successfully.");
        }
    }
}
