using Finance.Application.ViewModel;
using Finance.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Interfaces
{
    public interface IUserService : IBaseService<User, UserCreateDto, UserSelectDto>
    {
        Task<UserSelectDto?> ValidLoginUserAsync(string email, string password);
    }
}
