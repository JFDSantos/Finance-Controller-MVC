using Finance.Domain.Models.Enums;

namespace Finance.Application.ViewModel
{
    public class UserSelectDto
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required EUserRole Role { get; set; }
    }
}
