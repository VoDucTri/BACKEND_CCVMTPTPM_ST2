using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhom5_webAPI.Models;
using nhom5_webAPI.Repositories;

namespace nhom5_webAPI.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Lấy danh sách tất cả sản phẩm 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productRepository.GetAllProductsWithDetailsAsync();
            if (products == null || !products.Any())
            {
                return NotFound(new { Error = "No products found." });
            }

            return Ok(products);
        }

        // Lấy thông tin sản phẩm theo ID 
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            if (id <= 0)
                return BadRequest(new { Error = "Invalid Product ID." });

            var product = await _productRepository.GetProductWithDetailsByIdAsync(id);
            if (product == null)
                return NotFound(new { Error = "Product not found." });

            return Ok(product);
        }

        // Thêm mới một sản phẩm
        [Authorize(Policy = "Product.Create")]
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra SupplyCategoryId hợp lệ
            var supplyCategory = await _productRepository.GetSupplyCategoryByIdAsync(product.SupplyCategoryId);
            if (supplyCategory == null)
                return BadRequest(new { Error = "Invalid SupplyCategory ID." });

            // Gắn SupplyCategory
            product.SupplyCategory = supplyCategory;

            // Kiểm tra danh sách ảnh
            if (product.Images == null || !product.Images.Any())
            {
                return BadRequest(new { Error = "At least one image is required." });
            }

            foreach (var image in product.Images)
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
                // Thêm sản phẩm vào cơ sở dữ liệu
                await _productRepository.AddAsync(product);
                await _productRepository.SaveAsync();

                return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, new
                {
                    product.ProductId,
                    product.Name,
                    product.Price,
                    product.Quantity,
                    product.Description,
                    product.SupplyCategoryId,
                    Images = product.Images.Select(img => img.ImageUrl)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while saving the product.", Details = ex.Message });
            }
        }

        // Cập nhật thông tin sản phẩm 
        [Authorize(Policy = "Product.Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.ProductId)
                return BadRequest(new { Error = "Product ID mismatch." });

            // Lấy thông tin sản phẩm hiện tại
            var existingProduct = await _productRepository.GetProductWithDetailsByIdAsync(id);
            if (existingProduct == null)
                return NotFound(new { Error = "Product not found." });

            // Kiểm tra SupplyCategoryId hợp lệ
            var supplyCategory = await _productRepository.GetSupplyCategoryByIdAsync(product.SupplyCategoryId);
            if (supplyCategory == null)
                return BadRequest(new { Error = "Invalid SupplyCategory ID." });

            // Cập nhật thông tin cơ bản
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Quantity = product.Quantity;
            existingProduct.Description = product.Description;
            existingProduct.SupplyCategory = supplyCategory;

            // Cập nhật danh sách hình ảnh
            if (product.Images != null && product.Images.Any())
            {
                foreach (var image in product.Images)
                {
                    // Kiểm tra xem URL đã tồn tại chưa
                    var existingImage = existingProduct.Images
                        .FirstOrDefault(img => img.ImageUrl == image.ImageUrl);

                    if (existingImage == null)
                    {
                        // Nếu URL chưa tồn tại, thêm mới hình ảnh
                        existingProduct.Images.Add(new ProductImages
                        {
                            ProductId = id,
                            ImageUrl = image.ImageUrl
                        });
                    }
                }

                // Loại bỏ các hình ảnh không còn trong danh sách yêu cầu
                var imagesToRemove = existingProduct.Images
                    .Where(existingImg => !product.Images.Any(newImg => newImg.ImageUrl == existingImg.ImageUrl))
                    .ToList();

                foreach (var imageToRemove in imagesToRemove)
                {
                    existingProduct.Images.Remove(imageToRemove);
                }
            }

            try
            {
                // Cập nhật sản phẩm vào cơ sở dữ liệu
                _productRepository.Update(existingProduct);
                await _productRepository.SaveAsync();

                return Ok(new
                {
                    Message = "Product updated successfully.",
                    Product = new
                    {
                        existingProduct.ProductId,
                        existingProduct.Name,
                        existingProduct.Price,
                        existingProduct.Quantity,
                        existingProduct.Description,
                        existingProduct.SupplyCategoryId,
                        Images = existingProduct.Images.Select(img => new
                        {
                            img.ProductImageId,
                            img.ProductId,
                            img.ImageUrl
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while updating the product.", Details = ex.Message });
            }
        }

        // Xóa sản phẩm 
        [Authorize(Policy = "Product.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
                return BadRequest(new { Error = "Invalid Product ID." });

            // Tìm sản phẩm theo ID
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { Error = "Product not found." });

            // Xóa sản phẩm
            _productRepository.Delete(product);
            await _productRepository.SaveAsync();

            return NoContent();
        }
    }
}