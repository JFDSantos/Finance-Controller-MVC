using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserSelectDto> GetByIdAsync(int id);
        Task<UserSelectDto?> ValidLoginUserAsync(string email, string password);
        Task<UserSelectDto> RegisterAsync(UserCreateDto dto);
        Task DeleteAccountAsync(int userId);
    }
}
