using Finance.Application.ViewModel;
using Finance.Infrastructure.Data;
using Finance.Application;
using Finance.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Finance.Domain.Models;

namespace Finance.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FinanceContext _context;

        public UserRepository(FinanceContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.User.AddAsync(user);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.User.FindAsync(id);

            if (user != null) {
                _context.User.Remove(user);
            }
        }

        public async Task<IEnumerable<User?>> GetAllAsync()
        {
            return await _context.User.AsNoTracking().ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.User.AsNoTracking().FirstOrDefaultAsync(i => i.id == id);
        }

        public async Task UpdateAsync(int id, User user)
        {
            var userFind = await _context.User.FindAsync(id);

            if (userFind != null) {

                _context.Entry(userFind).CurrentValues.SetValues(user);
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.User.AsNoTracking().FirstOrDefaultAsync(i => i.email == email);
        }
    }
}
