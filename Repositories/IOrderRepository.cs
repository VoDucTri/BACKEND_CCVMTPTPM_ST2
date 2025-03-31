using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId); // Đổi int thành string
    }
}
