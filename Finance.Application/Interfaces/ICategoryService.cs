using Finance.Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategorySelectDto>> GetAllAsync();
        Task<CategorySelectDto> GetByIdAsync(int id);
        Task<CategorySelectDto> AddAsync(CategoryCreateDto dto);
        Task DeleteAsync(int id);
        Task<CategorySelectDto> UpdateAsync(int id, CategoryCreateDto dto);
    }
}
