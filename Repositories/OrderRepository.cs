using Microsoft.EntityFrameworkCore;
using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId) // So sánh đúng kiểu dữ liệu
                .Include(o => o.OrderDetails) // Include để lấy danh sách OrderDetails
                .ToListAsync();
        }
    }
}
