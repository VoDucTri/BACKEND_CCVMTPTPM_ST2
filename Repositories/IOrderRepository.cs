using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId); 
        Task<IEnumerable<Order>> GetOrdersByUsernameAsync(string username); 
        Task<IEnumerable<Order>> GetAllAsync(); 
    }
}
