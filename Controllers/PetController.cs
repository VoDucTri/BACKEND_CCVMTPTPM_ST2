using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom5_webAPI.Models;
using nhom5_webAPI.Repositories;

namespace nhom5_webAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PetController : ControllerBase
    {
        private readonly IPetRepository _petRepository;

        public PetController(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        // Lấy danh sách tất cả Pet
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllPets()
        {
            var pets = await _petRepository.GetAllAsync();
            var result = pets.Select(p => new
            {
                petId = p.PetId,
                name = p.Name,
                age = p.Age,
                price = p.Price,
                description = p.Description,
                categoryId = p.CategoryId,
                images = p.Images.Select(i => new
                {
                    imageId = i.Id,
                    url = i.ImageUrl
                }).ToList(),
                status = p.Status.ToString()
            });

            return Ok(result);
        }

        // Lấy thông tin Pet theo ID
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { Error = "Invalid Pet ID." });

            var pet = await _petRepository.GetByIdAsync(id);
            if (pet == null)
                return NotFound(new { Error = "Pet not found." });

            var result = new
            {
                petId = pet.PetId,
                name = pet.Name,
                age = pet.Age,
                price = pet.Price,
                description = pet.Description,
                categoryId = pet.CategoryId,
                images = pet.Images.Select(i => new
                {
                    imageId = i.Id,
                    url = i.ImageUrl
                }).ToList(),
                status = pet.Status.ToString()
            };

            return Ok(result);
        }

        // Lấy danh sách Pet theo CategoryId
        [AllowAnonymous]
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetPetsByCategory(int categoryId)
        {
            if (categoryId <= 0)
                return BadRequest(new { Error = "Invalid Category ID." });

            var pets = await _petRepository.GetPetsByCategoryAsync(categoryId);
            if (pets == null || !pets.Any())
                return NotFound(new { Error = "No pets found for this category." });

            return Ok(pets);
        }

        // Thêm mới một Pet
        [Authorize(Policy = "Pet.Create")] // Đổi từ Roles sang Policy
        [HttpPost]
        public async Task<IActionResult> AddPet([FromBody] Pet pet)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _petRepository.GetCategoryByIdAsync(pet.CategoryId);
            if (category == null)
                return BadRequest(new { Error = "Invalid Category ID." });

            pet.Category = category;

            if (pet.Images == null || !pet.Images.Any())
            {
                return BadRequest(new { Error = "At least one image is required." });
            }

            foreach (var image in pet.Images)
            {
                if (string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    return BadRequest(new { Error = "ImageUrl cannot be empty." });
                }

                if (!Uri.IsWellFormedUriString(image.ImageUrl, UriKind.Absolute))
                {
                    return BadRequest(new { Error = $"Invalid ImageUrl: {image.ImageUrl}" });
                }
            }

            try
            {
                await _petRepository.AddAsync(pet);
                await _petRepository.SaveAsync();

                return CreatedAtAction(nameof(GetPetById), new { id = pet.PetId }, new
                {
                    pet.PetId,
                    pet.Name,
                    pet.Age,
                    pet.Price,
                    pet.Status,
                    pet.CategoryId,
                    Images = pet.Images.Select(img => img.ImageUrl)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while saving the pet.", Details = ex.Message });
            }
        }

        // Cập nhật thông tin Pet
        [Authorize(Policy = "Pet.Edit")] // Đổi từ Roles sang Policy
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(int id, [FromBody] Pet pet)
        {
            pet.PetId = id;
            var existingPet = await _petRepository.GetByIdAsync(id);
            if (existingPet == null)
                return NotFound(new { Error = "Pet not found." });

            var category = await _petRepository.GetCategoryByIdAsync(pet.CategoryId);
            if (category == null)
                return BadRequest(new { Error = "Invalid Category ID." });

            existingPet.Name = pet.Name;
            existingPet.Age = pet.Age;
            existingPet.Price = pet.Price;
            existingPet.Description = pet.Description;
            existingPet.Status = pet.Status;
            existingPet.CategoryId = pet.CategoryId;
            existingPet.Category = category;

            if (pet.Images != null && pet.Images.Any())
            {
                foreach (var image in pet.Images)
                {
                    if (!existingPet.Images.Any(i => i.ImageUrl == image.ImageUrl))
                    {
                        existingPet.Images.Add(new PetImages
                        {
                            PetId = id,
                            ImageUrl = image.ImageUrl
                        });
                    }
                }

                var imagesToRemove = existingPet.Images
                    .Where(i => !pet.Images.Any(newImg => newImg.ImageUrl == i.ImageUrl))
                    .ToList();

                foreach (var imageToRemove in imagesToRemove)
                {
                    existingPet.Images.Remove(imageToRemove);
                }
            }

            _petRepository.Update(existingPet);
            await _petRepository.SaveAsync();

            return Ok(new
            {
                existingPet.PetId,
                existingPet.Name,
                existingPet.Age,
                existingPet.Price,
                existingPet.Description,
                existingPet.Status,
                existingPet.CategoryId,
                Images = existingPet.Images.Select(img => img.ImageUrl)
            });
        }

        // Xóa Pet
        [Authorize(Policy = "Pet.Delete")] // Đổi từ Roles sang Policy
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            if (id <= 0)
                return BadRequest(new { Error = "Invalid Pet ID." });

            var pet = await _petRepository.GetByIdAsync(id);
            if (pet == null)
                return NotFound(new { Error = "Pet not found." });

            _petRepository.Delete(pet);
            await _petRepository.SaveAsync();

            return NoContent();
        }
    }
}
