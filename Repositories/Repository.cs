using Microsoft.EntityFrameworkCore;
using nhom5_webAPI.Models;
using System.Linq.Expressions;

namespace nhom5_webAPI.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Triển khai phương thức mới (chỉ áp dụng nếu T là User)
        public async Task<T?> GetByUserNameAsync(string username)
        {
            if (typeof(T) == typeof(User))
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username) as T;
            }
            throw new InvalidOperationException("This method is only available for User entities.");
        }
    }
}
