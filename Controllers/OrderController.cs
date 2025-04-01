using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhom5_webAPI.Models;
using nhom5_webAPI.Repositories;
using System.Security.Claims;

namespace nhom5_webAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Đảm bảo chỉ người dùng đã xác thực mới có thể truy cập
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IPetRepository _petRepository;
        private readonly IProductRepository _productRepository;

        public OrderController(
            IOrderRepository orderRepository,
            IRepository<User> userRepository,
            IPetRepository petRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _petRepository = petRepository;
            _productRepository = productRepository;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);  // Lấy UserId từ Claims
        }

        [HttpGet]
        [Authorize(Policy = "Order.View")]
        public async Task<IActionResult> GetAllOrders()
        {
            if (User.IsInRole("User"))
            {
                var userId = User.Identity.Name;
                var userOrders = await _orderRepository.GetOrdersByUserIdAsync(userId); // Đổi tên biến orders thành userOrders 
                return Ok(userOrders);
            }

            var allOrders = await _orderRepository.GetAllAsync(); // Đổi tên biến orders thành allOrders
            return Ok(allOrders);

        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Order.View")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            // Kiểm tra xem user có quyền xem đơn hàng này không
            if (User.IsInRole("User") && order.UserId != User.Identity.Name)
            {
                return Forbid(); // Người dùng không thể xem đơn hàng của người khác
            }

            return Ok(order);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Policy = "Order.View")]
        public async Task<IActionResult> GetOrdersByUser(string userId)
        {
            // Kiểm tra xem user có quyền xem đơn hàng của người khác không
            if (User.IsInRole("User") && userId != User.Identity.Name)
            {
                return Forbid(); // Người dùng không thể xem đơn hàng của người khác
            }

            var userOrders = await _orderRepository.GetOrdersByUserIdAsync(userId); // Lấy đơn hàng theo userId
            return Ok(userOrders);
        }

        [HttpPost]
        [Authorize(Policy = "Order.Create")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                // Kiểm tra xem người dùng có quyền tạo đơn hàng cho chính mình không
                if (User.IsInRole("User") && order.UserId != User.Identity.Name)
                {
                    return Forbid(); // Người dùng không thể tạo đơn hàng cho người khác
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return BadRequest(new { Errors = errors });
                }

                var user = await _userRepository.GetByUserNameAsync(order.UserId);
                if (user == null)
                    return BadRequest(new { Error = "Invalid UserId." });

                order.User = user;
                order.UserId = user.Id;
                order.OrderDate = DateTime.UtcNow;
                order.Status = "Pending";

                if (order.OrderDetails == null || !order.OrderDetails.Any())
                    return BadRequest(new { Error = "Order must contain at least one item." });

                decimal totalPrice = 0;

                foreach (var detail in order.OrderDetails)
                {
                    if (string.IsNullOrEmpty(detail.ProductType))
                        return BadRequest(new { Error = "ProductType is required." });

                    switch (detail.ProductType.ToLower())
                    {
                        case "pet":
                            if (!detail.PetId.HasValue)
                                return BadRequest(new { Error = "PetId is required for pet orders." });
                            var pet = await _petRepository.GetByIdAsync(detail.PetId.Value);
                            if (pet == null)
                                return BadRequest(new { Error = $"Pet with ID {detail.PetId.Value} not found." });

                            if (detail.Price != pet.Price)
                                return BadRequest(new { Error = $"Invalid price for pet ID {detail.PetId.Value}" });
                            break;

                        case "product":
                            if (!detail.ProductId.HasValue)
                                return BadRequest(new { Error = "ProductId is required for product orders." });
                            var product = await _productRepository.GetByIdAsync(detail.ProductId.Value);
                            if (product == null)
                                return BadRequest(new { Error = $"Product with ID {detail.ProductId.Value} not found." });

                            if (detail.Price != product.Price)
                                return BadRequest(new { Error = $"Invalid price for product ID {detail.ProductId.Value}" });

                            if (detail.Quantity > product.Quantity)
                                return BadRequest(new { Error = $"Product ID {detail.ProductId.Value} only has {product.Quantity} available" });
                            break;

                        default:
                            return BadRequest(new { Error = $"Invalid ProductType: {detail.ProductType}. Allowed values are 'Pet' and 'Product'." });
                    }

                    if (detail.Quantity <= 0)
                        return BadRequest(new { Error = "Quantity must be greater than 0." });
                    if (detail.Price <= 0)
                        return BadRequest(new { Error = "Price must be greater than 0." });

                    totalPrice += detail.Price * detail.Quantity;
                    detail.Order = order;
                }

                if (order.TotalPrice != totalPrice)
                    return BadRequest(new { Error = $"Invalid total price. Expected: {totalPrice}, Received: {order.TotalPrice}" });

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveAsync();

                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = order.OrderId },
                    new
                    {
                        Message = "Order created successfully",
                        OrderId = order.OrderId,
                        Status = order.Status,
                        TotalPrice = order.TotalPrice
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "An error occurred while processing your order.",
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Order.Edit")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            try
            {
                if (id != order.OrderId)
                    return BadRequest(new { Error = "Order ID mismatch." });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return BadRequest(new { Errors = errors });
                }

                var existingOrder = await _orderRepository.GetByIdAsync(id);
                if (existingOrder == null)
                    return NotFound(new { Error = $"Order with ID {id} not found." });

                var user = await _userRepository.GetByUserNameAsync(order.UserId);
                if (user == null)
                    return BadRequest(new { Error = "Invalid UserId." });

                existingOrder.UserId = user.Id;
                existingOrder.Status = order.Status;

                if (order.OrderDetails == null || !order.OrderDetails.Any())
                    return BadRequest(new { Error = "Order must contain at least one item." });

                decimal totalPrice = 0;
                var updatedDetails = new List<OrderDetail>();

                foreach (var detail in order.OrderDetails)
                {
                    if (string.IsNullOrEmpty(detail.ProductType))
                        return BadRequest(new { Error = "ProductType is required." });

                    switch (detail.ProductType.ToLower())
                    {
                        case "pet":
                            if (!detail.PetId.HasValue)
                                return BadRequest(new { Error = "PetId is required for pet orders." });

                            var pet = await _petRepository.GetByIdAsync(detail.PetId.Value);
                            if (pet == null)
                                return BadRequest(new { Error = $"Pet with ID {detail.PetId.Value} not found." });

                            if (detail.Price != pet.Price)
                                return BadRequest(new { Error = $"Invalid price for pet ID {detail.PetId.Value}" });
                            break;

                        case "product":
                            if (!detail.ProductId.HasValue)
                                return BadRequest(new { Error = "ProductId is required for product orders." });

                            var product = await _productRepository.GetByIdAsync(detail.ProductId.Value);
                            if (product == null)
                                return BadRequest(new { Error = $"Product with ID {detail.ProductId.Value} not found." });

                            if (detail.Price != product.Price)
                                return BadRequest(new { Error = $"Invalid price for product ID {detail.ProductId.Value}" });

                            if (detail.Quantity > product.Quantity)
                                return BadRequest(new { Error = $"Product ID {detail.ProductId.Value} only has {product.Quantity} available" });
                            break;

                        default:
                            return BadRequest(new { Error = $"Invalid ProductType: {detail.ProductType}. Allowed values are 'Pet' and 'Product'." });
                    }

                    if (detail.Quantity <= 0)
                        return BadRequest(new { Error = "Quantity must be greater than 0." });
                    if (detail.Price <= 0)
                        return BadRequest(new { Error = "Price must be greater than 0." });

                    totalPrice += detail.Price * detail.Quantity;

                    updatedDetails.Add(new OrderDetail
                    {
                        OrderId = id,
                        ProductType = detail.ProductType,
                        ProductId = detail.ProductId,
                        PetId = detail.PetId,
                        Quantity = detail.Quantity,
                        Price = detail.Price
                    });
                }

                if (existingOrder.OrderDetails == null)
                {
                    existingOrder.OrderDetails = new List<OrderDetail>();
                }

                existingOrder.OrderDetails.Clear();
                foreach (var newDetail in updatedDetails)
                {
                    existingOrder.OrderDetails.Add(newDetail);
                }

                if (order.TotalPrice != totalPrice)
                    return BadRequest(new { Error = $"Invalid total price. Expected: {totalPrice}, Received: {order.TotalPrice}" });

                existingOrder.TotalPrice = totalPrice;

                _orderRepository.Update(existingOrder);
                await _orderRepository.SaveAsync();

                return Ok(new
                {
                    Message = "Order updated successfully",
                    OrderId = existingOrder.OrderId,
                    Status = existingOrder.Status,
                    TotalPrice = existingOrder.TotalPrice,
                    OrderDetails = updatedDetails.Select(d => new
                    {
                        d.ProductType,
                        d.ProductId,
                        d.PetId,
                        d.Quantity,
                        d.Price
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "An error occurred while updating the order.",
                    Details = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Order.Delete")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            _orderRepository.Delete(order);
            await _orderRepository.SaveAsync();
            return NoContent();
        }

    }
}
