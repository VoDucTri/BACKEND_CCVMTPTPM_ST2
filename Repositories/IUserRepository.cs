using nhom5_webAPI.Models;

namespace nhom5_webAPI.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUserNameAsync(string userName); 
    }
}
