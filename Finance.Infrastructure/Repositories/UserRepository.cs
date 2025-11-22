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

        public async Task AddAsync(User dto)
        {
            _context.Add(dto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int IdTransaction)
        {
            var user = _context.User.Find(IdTransaction);

            if (user != null) {
                _context.Remove(user);
                await _context.SaveChangesAsync();
            }

            throw new KeyNotFoundException("User not found");
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var users = await _context.User.Select(u => new User {

                id = u.id,
                user = u.user,
                email = u.email,
                role = u.role,
                password = u.password

            }).ToListAsync();

            if(users != null)
            {
                return users;
            }
            
            throw new KeyNotFoundException("Users not found");
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var user = await _context.User.Select(u => new User
            {

                id = u.id,
                user = u.user,
                email = u.email,
                role = u.role,
                password = u.password

            }).FirstOrDefaultAsync(i => i.id == id);

            if (user != null)
            {
                return user;
            }

            throw new KeyNotFoundException("User not found");
        }

        public async Task<User> UpdateAsync(int IdTransaction, User dto)
        {
            var user = _context.User.Find(IdTransaction);

            if (user != null) {

                user.user = dto.user;
                user.email = dto.email;
                user.password = dto.password;
                user.role = dto.role;

                _context.Update(user);
                await _context.SaveChangesAsync();

                return new User
                {
                    id = user.id,
                    user = user.user,
                    email = user.email,
                    role = user.role,
                    password = user.password
                };
            }

            throw new KeyNotFoundException("User not Found");
        }

        public async Task<User> ValidLoginUser(string email, string password)
        {
            var user = await _context.User.FirstOrDefaultAsync(i => i.email == email);

            if (user != null) 
            {
                var passIsValid = BCrypt.Net.BCrypt.Verify(password, user.password);

                if (passIsValid) 
                {
                    return new User
                    {
                        id = user.id,
                        user = user.user,
                        email = user.email,
                        role = user.role,
                        password = user.password
                    };
                }

                throw new KeyNotFoundException("Email or Password is incorrect");
            }

            throw new KeyNotFoundException("User not found");
        }
    }
}
