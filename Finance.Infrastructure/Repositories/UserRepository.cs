using Finance.Application.ViewModel;
using Finance.Infrastructure.Data;
using Finance.Application;
using Finance.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Finance.Domain.Models;

namespace Finance.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User, int>, IUserRepository
    {
        public UserRepository(FinanceContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.User.AsNoTracking().FirstOrDefaultAsync(i => i.email == email);
        }
    }
}
