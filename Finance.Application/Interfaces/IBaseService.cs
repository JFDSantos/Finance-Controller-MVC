using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Interfaces
{
    public interface IBaseService<TEntity, TCreateDto, TSelectDto>
    {
        Task<IEnumerable<TSelectDto>> GetAllAsync();
        Task<TSelectDto> GetByIdAsync(int id);
        Task<TSelectDto> AddAsync(TCreateDto dto);
        Task<TSelectDto> UpdateAsync(int id, TCreateDto dto);
        Task DeleteAsync(int id);
    }
}
