using Finance.Web.Models.Enums;

namespace Finance.Web.ViewModel
{
    public class UserCreateDto
    {
        public required string user { get; set; }
        public required string password { get; set; }
        public required string email { get; set; }
        public required EUserRole role { get; set; }
    }
}
