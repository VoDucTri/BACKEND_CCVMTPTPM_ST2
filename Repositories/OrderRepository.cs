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
        public async Task<IEnumerable<Order>> GetOrdersByUsernameAsync(string username)
        {
            // Tìm userId từ username trong bảng Users
            var user = await _context.Users
                .Where(u => u.UserName == username)
                .Select(u => u.Id) // Giả sử cột Id trong bảng Users chứa userId
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new List<Order>(); // Trả về mảng rỗng nếu không tìm thấy user
            }

            // Lấy đơn hàng dựa trên userId
            return await _context.Orders
                .Where(o => o.UserId == user)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }
    }
}
