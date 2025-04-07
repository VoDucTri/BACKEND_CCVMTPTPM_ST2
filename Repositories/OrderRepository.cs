using Microsoft.EntityFrameworkCore;
using nhom5_webAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nhom5_webAPI.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderByIdWithDetailsAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null && order.OrderDetails == null)
            {
                order.OrderDetails = new List<OrderDetail>();
            }

            return order;
        }

        public new async Task<IEnumerable<Order>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.User)
                .ToListAsync();

            foreach (var order in orders)
            {
                if (order.OrderDetails == null)
                {
                    order.OrderDetails = new List<OrderDetail>();
                }
            }

            return orders;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .Include(o => o.User)
                .ToListAsync();

            foreach (var order in orders)
            {
                if (order.OrderDetails == null)
                {
                    order.OrderDetails = new List<OrderDetail>();
                }
            }

            return orders;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUsernameAsync(string username)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.User)
                .Where(o => o.User != null && o.User.UserName == username)
                .ToListAsync();

            foreach (var order in orders)
            {
                if (order.OrderDetails == null)
                {
                    order.OrderDetails = new List<OrderDetail>();
                }
            }

            return orders;
        }
    }
}