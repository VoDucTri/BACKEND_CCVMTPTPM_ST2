using nhom5_webAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nhom5_webAPI.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersByUsernameAsync(string username);
        new Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetOrderByIdWithDetailsAsync(int id);
    }
}