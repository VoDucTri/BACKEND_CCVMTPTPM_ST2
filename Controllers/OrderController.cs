using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhom5_webAPI.Models;
using nhom5_webAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace nhom5_webAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        #region DTOs
        public class OrderDTO
        {
            public int OrderId { get; set; }
            public string UserId { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalPrice { get; set; }
            public string Status { get; set; }
            public UserDTO User { get; set; }
            public List<OrderDetailDTO> OrderDetails { get; set; }
        }

        public class UserDTO
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public bool EmailConfirmed { get; set; }
        }

        public class OrderDetailDTO
        {
            public int Id { get; set; }
            public string ProductType { get; set; }
            public int? ProductId { get; set; }
            public int? PetId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
        #endregion

        // Phương thức lấy username từ token
        private string GetUsername()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            return username;
        }

        // Phương thức chuyển đổi từ Order sang OrderDTO
        private OrderDTO ConvertToDTO(Order order)
        {
            if (order == null)
                return null;

            return new OrderDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                User = order.User != null ? new UserDTO
                {
                    Id = order.User.Id,
                    UserName = order.User.UserName,
                    Email = order.User.Email,
                    EmailConfirmed = order.User.EmailConfirmed
                } : null,
                OrderDetails = order.OrderDetails?.Select(d => new OrderDetailDTO
                {
                    Id = d.Id,
                    ProductType = d.ProductType,
                    ProductId = d.ProductId,
                    PetId = d.PetId,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList() ?? new List<OrderDetailDTO>()
            };
        }

        [HttpGet]
        [Authorize(Policy = "Order.View")]
        public async Task<IActionResult> GetAllOrders()
        {
            var username = GetUsername();
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { Message = "Không thể lấy username từ token." });
            }

            IEnumerable<Order> orders;

            if (User.IsInRole("User"))
            {
                orders = await _orderRepository.GetOrdersByUsernameAsync(username);
            }
            else
            {
                orders = await _orderRepository.GetAllAsync();
            }

            if (orders == null || !orders.Any())
            {
                return Ok(new List<OrderDTO>()); // Trả về mảng rỗng thay vì 404
            }

            var orderDTOs = orders.Select(ConvertToDTO).ToList();
            return Ok(orderDTOs);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Order.View")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            // Sử dụng phương thức mới để lấy đơn hàng với chi tiết
            var order = await _orderRepository.GetOrderByIdWithDetailsAsync(id);

            if (order == null)
                return NotFound(new { status = "not_found", message = "Order not found." });

            if (User.IsInRole("User") && order.UserId != User.Identity.Name)
            {
                return StatusCode(403, new { status = "forbidden", message = "You cannot access this order." });
            }

            var orderDTO = ConvertToDTO(order);
            return Ok(orderDTO);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Policy = "Order.View")]
        public async Task<IActionResult> GetOrdersByUser(string userId)
        {
            if (User.IsInRole("User") && userId != User.Identity.Name)
            {
                return StatusCode(403, new { status = "forbidden", message = "You cannot access this user's orders." });
            }

            var userOrders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            if (userOrders == null || !userOrders.Any())
                return Ok(new List<OrderDTO>()); // Trả về mảng rỗng thay vì 404

            var orderDTOs = userOrders.Select(ConvertToDTO).ToList();
            return Ok(orderDTOs);
        }

        [HttpPost]
        [Authorize(Policy = "Order.Create")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                if (User.IsInRole("User") && order.UserId != User.Identity.Name)
                {
                    return StatusCode(403, new { status = "forbidden", message = "You cannot create an order for another user." });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return BadRequest(new { status = "invalid", errors = errors });
                }

                var user = await _userRepository.GetByUserNameAsync(order.UserId);
                if (user == null)
                    return BadRequest(new { status = "invalid_user", message = "Invalid UserId." });

                order.User = user;
                order.OrderDate = DateTime.UtcNow;
                order.Status = "Pending";

                if (order.OrderDetails == null || !order.OrderDetails.Any())
                    return BadRequest(new { status = "empty_order", message = "Order must contain at least one item." });

                decimal totalPrice = 0;
                foreach (var detail in order.OrderDetails)
                {
                    if (string.IsNullOrEmpty(detail.ProductType))
                        return BadRequest(new { status = "invalid_product_type", message = "ProductType is required." });

                    switch (detail.ProductType.ToLower())
                    {
                        case "pet":
                            if (!detail.PetId.HasValue)
                                return BadRequest(new { status = "invalid_pet_id", message = "PetId is required for pet orders." });

                            var pet = await _petRepository.GetByIdAsync(detail.PetId.Value);
                            if (pet == null)
                                return BadRequest(new { status = "pet_not_found", message = $"Pet with ID {detail.PetId.Value} not found." });

                            if (detail.Price != pet.Price)
                                return BadRequest(new { status = "invalid_pet_price", message = $"Invalid price for pet ID {detail.PetId.Value}" });
                            break;

                        case "product":
                            if (!detail.ProductId.HasValue)
                                return BadRequest(new { status = "invalid_product_id", message = "ProductId is required for product orders." });

                            var product = await _productRepository.GetByIdAsync(detail.ProductId.Value);
                            if (product == null)
                                return BadRequest(new { status = "product_not_found", message = $"Product with ID {detail.ProductId.Value} not found." });

                            if (detail.Price != product.Price)
                                return BadRequest(new { status = "invalid_product_price", message = $"Invalid price for product ID {detail.ProductId.Value}" });

                            if (detail.Quantity > product.Quantity)
                                return BadRequest(new { status = "insufficient_product_quantity", message = $"Product ID {detail.ProductId.Value} only has {product.Quantity} available" });
                            break;

                        default:
                            return BadRequest(new { status = "invalid_product_type", message = $"Invalid ProductType: {detail.ProductType}. Allowed values are 'Pet' and 'Product'." });
                    }

                    if (detail.Quantity <= 0)
                        return BadRequest(new { status = "invalid_quantity", message = "Quantity must be greater than 0." });
                    if (detail.Price <= 0)
                        return BadRequest(new { status = "invalid_price", message = "Price must be greater than 0." });

                    totalPrice += detail.Price * detail.Quantity;
                    detail.Order = order;
                }

                if (order.TotalPrice != totalPrice)
                    return BadRequest(new { status = "invalid_total_price", message = $"Invalid total price. Expected: {totalPrice}, Received: {order.TotalPrice}" });

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveAsync();

                // Lấy lại order với OrderDetails để trả về
                var createdOrder = await _orderRepository.GetOrderByIdWithDetailsAsync(order.OrderId);
                var orderDTO = ConvertToDTO(createdOrder);

                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = order.OrderId },
                    new
                    {
                        status = "success",
                        message = "Order created successfully",
                        order = orderDTO
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "An error occurred while processing your order.",
                    details = ex.Message
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
                    return BadRequest(new { status = "id_mismatch", message = "Order ID mismatch." });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return BadRequest(new { status = "invalid", errors = errors });
                }

                var existingOrder = await _orderRepository.GetByIdAsync(id);
                if (existingOrder == null)
                    return NotFound(new { status = "not_found", message = $"Order with ID {id} not found." });

                var user = await _userRepository.GetByUserNameAsync(order.UserId);
                if (user == null)
                    return BadRequest(new { status = "invalid_user", message = "Invalid UserId." });

                existingOrder.UserId = user.Id;
                existingOrder.Status = order.Status;

                if (order.OrderDetails == null || !order.OrderDetails.Any())
                    return BadRequest(new { status = "empty_order", message = "Order must contain at least one item." });

                decimal totalPrice = 0;
                var updatedDetails = new List<OrderDetail>();

                foreach (var detail in order.OrderDetails)
                {
                    if (string.IsNullOrEmpty(detail.ProductType))
                        return BadRequest(new { status = "invalid_product_type", message = "ProductType is required." });

                    switch (detail.ProductType.ToLower())
                    {
                        case "pet":
                            if (!detail.PetId.HasValue)
                                return BadRequest(new { status = "invalid_pet_id", message = "PetId is required for pet orders." });

                            var pet = await _petRepository.GetByIdAsync(detail.PetId.Value);
                            if (pet == null)
                                return BadRequest(new { status = "pet_not_found", message = $"Pet with ID {detail.PetId.Value} not found." });

                            if (detail.Price != pet.Price)
                                return BadRequest(new { status = "invalid_pet_price", message = $"Invalid price for pet ID {detail.PetId.Value}" });
                            break;

                        case "product":
                            if (!detail.ProductId.HasValue)
                                return BadRequest(new { status = "invalid_product_id", message = "ProductId is required for product orders." });

                            var product = await _productRepository.GetByIdAsync(detail.ProductId.Value);
                            if (product == null)
                                return BadRequest(new { status = "product_not_found", message = $"Product with ID {detail.ProductId.Value} not found." });

                            if (detail.Price != product.Price)
                                return BadRequest(new { status = "invalid_product_price", message = $"Invalid price for product ID {detail.ProductId.Value}" });

                            if (detail.Quantity > product.Quantity)
                                return BadRequest(new { status = "insufficient_product_quantity", message = $"Product ID {detail.ProductId.Value} only has {product.Quantity} available" });
                            break;

                        default:
                            return BadRequest(new { status = "invalid_product_type", message = $"Invalid ProductType: {detail.ProductType}. Allowed values are 'Pet' and 'Product'." });
                    }

                    if (detail.Quantity <= 0)
                        return BadRequest(new { status = "invalid_quantity", message = "Quantity must be greater than 0." });
                    if (detail.Price <= 0)
                        return BadRequest(new { status = "invalid_price", message = "Price must be greater than 0." });

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
                else
                {
                    existingOrder.OrderDetails.Clear();
                }

                foreach (var newDetail in updatedDetails)
                {
                    existingOrder.OrderDetails.Add(newDetail);
                }

                if (order.TotalPrice != totalPrice)
                    return BadRequest(new { status = "invalid_total_price", message = $"Invalid total price. Expected: {totalPrice}, Received: {order.TotalPrice}" });

                existingOrder.TotalPrice = totalPrice;

                _orderRepository.Update(existingOrder);
                await _orderRepository.SaveAsync();

                // Lấy lại order với OrderDetails để trả về
                var updatedOrder = await _orderRepository.GetOrderByIdWithDetailsAsync(id);
                var orderDTO = ConvertToDTO(updatedOrder);

                return Ok(new
                {
                    status = "success",
                    message = "Order updated successfully",
                    order = orderDTO
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "An error occurred while updating the order.",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Order.Delete")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { status = "not_found", message = "Order not found." });

            _orderRepository.Delete(order);
            await _orderRepository.SaveAsync();
            return Ok(new { status = "success", message = "Order deleted successfully" });
        }
    }
}