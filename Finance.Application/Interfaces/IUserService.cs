using Finance.Application.ViewModel;
using Finance.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserSelectDto>> GetAllAsync();
        Task<UserSelectDto> GetByIdAsync(int id);
        Task<UserSelectDto> AddAsync(UserCreateDto dto);
        Task DeleteAsync(int IdTransaction);
        Task<UserSelectDto> UpdateAsync(int IdTransaction, UserCreateDto dto);
        Task<UserSelectDto> ValidLoginUser(string email, string password);
    }
}
